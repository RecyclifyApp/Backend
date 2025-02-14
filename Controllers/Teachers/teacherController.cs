using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Newtonsoft.Json;

namespace Backend.Controllers.Teachers
{
    [ApiController]
    [Route("api/[controller]")]

    public class TeacherController(MyDbContext context, HttpClient httpClient) : ControllerBase
    {
        private readonly MyDbContext _context = context;
        private readonly HttpClient _httpClient = httpClient;

        // Get Classes
        [HttpGet("get-classes")]
        public async Task<IActionResult> GetClasses(string teacherID)
        {
            if (string.IsNullOrEmpty(teacherID))
            {
                return BadRequest(new { error = "UERROR: Invalid teacher ID. Please provide a valid teacher ID." });
            }

            try
            {
                var classes = await _context.Classes
                .Where(c => c.TeacherID == teacherID)
                .OrderBy(c => c.ClassName)
                .ToListAsync();

                if (classes == null || classes.Count == 0)
                {
                    classes = [];
                    return Ok(new { message = "SUCCESS: No classes found.", data = classes });
                }

                return Ok(new { message = "SUCCESS: Classes found.", data = classes });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Overall Classes Data
        [HttpGet("get-overall-classes-data")]
        public async Task<IActionResult> GetOverallClassesData()
        {
            try
            {
                var classes = await _context.Classes.ToListAsync();
                if (classes == null || classes.Count == 0)
                {
                    classes = [];
                    return Ok(new { message = "SUCCESS: No classes found.", data = classes });
                }

                return Ok(new { message = "SUCCESS: Classes found.", data = classes });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Class
        [HttpGet("get-class")]
        public async Task<IActionResult> GetClass(string classID)
        {
            if (string.IsNullOrEmpty(classID))
            {
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try
            {
                var classData = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classID);
                if (classData == null)
                {
                    return NotFound(new { message = "SUCCESS: Class not found.", data = classData });
                }

                return Ok(new { message = "SUCCESS: Class found.", data = classData });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Create Class, (Add Class Image later)
        [HttpPost("create-class")]
        public async Task<IActionResult> CreateClass(string className, string classDescription, string teacherID)
        {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(teacherID))
            {
                return BadRequest(new { error = "UERROR: Invalid class details. Please provide valid class details." });
            }

            // Check if class name is an integer (E.g. 101)
            if (!int.TryParse(className, out int intClassName))
            {
                return BadRequest(new { error = "UERROR: Class name must be an integer." });
            }

            // Find Teacher
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherID);
            if (teacher == null)
            {
                return NotFound(new { error = "ERROR: Teacher not found." });
            }

            // Find Class Existance
            var classExist = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == intClassName);
            if (classExist != null)
            {
                return BadRequest(new { error = "UERROR: Class already exists." });
            }

            try
            {
                var classID = Utilities.GenerateUniqueID();
                var newClass = new Class
                {
                    ClassID = classID,
                    ClassName = intClassName,
                    ClassDescription = classDescription,
                    ClassImage = "",
                    ClassPoints = 0,
                    WeeklyClassPoints = [],
                    TeacherID = teacherID,
                    Teacher = teacher,
                    JoinCode = Utilities.GenerateRandomInt(100000, 999999)
                };

                _context.Classes.Add(newClass);

                var reccomendResponse = await ReccommendationsManager.RecommendQuestsAsync(_context, classID, 3);

                if (reccomendResponse != null)
                {
                    foreach (var quest in reccomendResponse.result)
                    {
                        var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == teacherID);
                        if (assignedTeacher == null)
                        {
                            return NotFound(new { error = "ERROR: Class's teacher not found" });
                        }

                        var questProgress = new QuestProgress
                        {
                            QuestID = quest.QuestID,
                            ClassID = classID,
                            DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                            AmountCompleted = 0,
                            Completed = false,
                            Quest = quest,
                            AssignedTeacherID = assignedTeacher.TeacherID,
                            AssignedTeacher = assignedTeacher
                        };

                        _context.QuestProgresses.Add(questProgress);
                    }
                }

                _context.SaveChanges();

                return Ok(new { message = "SUCCESS: Class created successfully." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }

        }

        // Delete Class 
        [HttpDelete("delete-class")]
        public async Task<IActionResult> DeleteClass(string classId)
        {
            if (string.IsNullOrEmpty(classId))
            {
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try
            {
                var classData = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classId);
                if (classData == null)
                {
                    return NotFound(new { error = "ERROR: Class not found." });
                }

                _context.Classes.Remove(classData);
                await _context.SaveChangesAsync();

                return Ok(new { message = "SUCCESS: Class deleted successfully." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Update Class, Add Class Image later
        [HttpPut("update-class")]
        public async Task<IActionResult> UpdateClass(string classId, string className, string classDescription)
        {
            if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(className))
            {
                return BadRequest(new { error = "UERROR: Invalid class details. Please provide valid class details." });
            }

            if (!int.TryParse(className, out int intClassName))
            {
                return BadRequest(new { error = "UERROR: Class name must be an integer." });
            }

            // Check if other class with same name exists, must have different ID
            var classExist = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == intClassName && c.ClassID != classId);
            if (classExist != null)
            {
                return BadRequest(new { error = "UERROR: Class already exists." });
            }

            try
            {
                var classData = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classId);
                if (classData == null)
                {
                    return NotFound(new { error = "ERROR: Class not found." });
                }

                classData.ClassName = intClassName;
                classData.ClassDescription = classDescription;
                _context.Classes.Update(classData);
                await _context.SaveChangesAsync();

                return Ok(new { message = "SUCCESS: Class updated successfully." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Student
        [HttpGet("get-students")]
        public async Task<IActionResult> GetStudents([FromQuery] string classId)
        {
            if (string.IsNullOrEmpty(classId))
            {
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try
            {
                var students = await _context.ClassStudents
                    .Where(cs => cs.ClassID == classId)
                    .Join(
                        _context.Students,
                        cs => cs.StudentID,
                        s => s.StudentID,
                        (cs, s) => new { cs, s }
                    )
                    .GroupJoin(
                        _context.Parents,
                        combined => combined.s.ParentID,
                        p => p.ParentID,
                        (combined, p) => new { combined, Parent = p.FirstOrDefault() }
                    )
                    .Join(
                        _context.Users,
                        combined => combined.combined.s.UserID,
                        u => u.Id,
                        (combined, u) => new
                        {
                            combined.combined.s.StudentID,
                            combined.combined.s.ParentID,
                            combined.combined.s.League,
                            combined.combined.s.LeagueRank,
                            combined.combined.s.CurrentPoints,
                            combined.combined.s.TotalPoints,
                            combined.combined.s.Streak,
                            combined.combined.s.LastClaimedStreak,
                            combined.combined.s.TaskProgresses,
                            combined.combined.s.Redemptions,
                            Parent = combined.Parent != null && combined.Parent.User != null
                                ? new
                                {
                                    ParentName = combined.Parent.User.Name,
                                    ParentEmail = combined.Parent.User.Email
                                }
                                : null,
                            User = new
                            {
                                u.Name,
                                u.Email
                            }
                        }
                    )
                    .OrderBy(student => student.User.Name)
                    .ToListAsync();

                if (students == null || students.Count == 0)
                {
                    return Ok(new { message = "SUCCESS: No students found.", data = new List<object>() });
                }

                return Ok(new { message = "SUCCESS: Students retrieved", data = students });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Delete Student
        [HttpDelete("delete-student")]
        public async Task<IActionResult> DeleteStudent(string studentID)
        {
            if (string.IsNullOrEmpty(studentID))
            {
                return BadRequest(new { error = "UERROR: Invalid student ID. Please provide a valid student ID." });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
            if (student == null)
            {
                return NotFound(new { error = "ERROR: Student not found." });
            }

            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return Ok(new { message = "SUCCESS: Student deleted successfully." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Update Student
        [HttpPut("update-student")]
        public async Task<IActionResult> UpdateStudent(string studentID, string studentName, string studentEmail)
        {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentEmail))
            {
                return BadRequest(new { error = "UERROR: Invalid student details. Please provide valid student details." });
            }

            // Find student and student user details
            var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentID == studentID);

            // Collated if clause to check student and student user details
            if (student == null || student.User == null)
            {
                return NotFound(new { error = "ERROR: Student not found." });
            }

            try
            {
                student.User.Name = studentName;
                student.User.Email = studentEmail;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();

                return Ok(new { message = "SUCCESS: Student updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Send Update Email to Recipient (Student / Parent)
        [HttpPost("send-update-email")]
        public async Task<IActionResult> SendUpdateEmail(
            [FromQuery] List<string> recipients,
            [FromQuery] string classID,
            [FromQuery] string studentID,
            [FromQuery] string studentEmail,
            [FromQuery] string? parentID = null,
            [FromQuery] string? parentEmail = null)
        {
            if (recipients == null || recipients.Count == 0 || string.IsNullOrEmpty(classID))
            {
                return BadRequest(new { error = "UERROR: Invalid recipients or class ID. Please provide valid recipients and class ID." });
            }

            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(studentEmail))
            {
                return BadRequest(new { error = "UERROR: Invalid student details. Please provide valid student details." });
            }

            if (recipients.Contains("parents") && (string.IsNullOrEmpty(parentID) || string.IsNullOrEmpty(parentEmail)))
            {
                return BadRequest(new { error = "UERROR: Invalid parent details. Please provide valid parent details." });
            }

            // Fetch class, student, and parent (if selected) user details
            var classDetails = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classID);
            var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentID == studentID);
            var parentUser = await _context.Parents
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ParentID == parentID);

            // Separate the recipients string by commas
            recipients = [.. recipients[0].Split(',')];

            if (classDetails == null)
            {
                return NotFound(new { error = "ERROR: Class not found." });
            }

            if (student == null || student.User == null)
            {
                return NotFound(new { error = "ERROR: Student not found." });
            }

            if (recipients.Contains("parents") && (parentUser == null || parentUser.User == null))
            {
                return NotFound(new { error = "ERROR: Parent not found." });
            }

            try
            {
                var emailVars = new Dictionary<string, string> {
                    { "studentName", student.User.Name },
                    { "email", studentEmail },
                    { "className", classDetails.ClassName.ToString() },
                    { "totalPoints", student.TotalPoints.ToString() },
                    { "currentPoints", student.CurrentPoints.ToString() },
                    { "league", student.League ?? string.Empty },
                    { "redemptions", student.Redemptions?.Count.ToString() ?? "0" }
                };

                if (parentUser != null && !string.IsNullOrEmpty(parentEmail))
                {
                    if (!string.IsNullOrEmpty(parentID))
                    {
                        emailVars.Add("parentID", parentID);
                    }
                    if (!string.IsNullOrEmpty(parentEmail))
                    {
                        emailVars.Add("parentEmail", parentEmail);
                    }
                    if (parentUser != null && parentUser.User != null)
                    {
                        emailVars.Add("parentName", parentUser.User.Name);
                    }
                }

                foreach (var recipient in recipients)
                {
                    if (!string.IsNullOrEmpty(studentEmail) && recipient == "students")
                    {
                        try
                        {
                            await Emailer.SendEmailAsync(studentEmail, "Update from Recyclify", "StudentUpdateEmail", emailVars);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
                        }
                    }
                    if (!string.IsNullOrEmpty(parentEmail) && recipient == "parents")
                    {
                        try
                        {
                            await Emailer.SendEmailAsync(parentEmail, "Update from Recyclify", "ParentUpdateEmail", emailVars);
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
                        }
                    }
                }

                return Ok(new { message = "SUCCESS: Email sent successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Tasks Waiting for Verification
        [HttpGet("get-all-tasks")]
        public async Task<IActionResult> GetAllTasks(string teacherID)
        {
            if (string.IsNullOrEmpty(teacherID))
            {
                return BadRequest(new { error = "UERROR: Invalid Teacher ID. Please provide a valid Teacher ID." });
            }

            try
            {
                var tasksQuery = _context.TaskProgresses
                    .Where(t => t.AssignedTeacherID == teacherID)
                    .Include(t => t.Student)
                    .ThenInclude(s => s!.User)
                    .Select(t => new
                    {
                        t.TaskID,
                        t.Task,
                        Student = t.Student != null && t.Student.User != null ? new
                        {
                            t.Student.StudentID,
                            t.Student.UserID,
                            t.Student.User.Name,
                            t.Student.User.Email,
                            t.Student.User.Avatar
                        } : null,

                        Class = _context.ClassStudents
                            .Where(cs => cs.StudentID == t.Student!.StudentID)
                            .Select(cs => new
                            {
                                cs.ClassID,
                                ClassName = cs.Class != null ? cs.Class.ClassName.ToString() : "Unknown Class"
                            })
                            .FirstOrDefault(),

                        t.DateAssigned,
                        t.TaskVerified,
                        t.TaskRejected,
                        t.VerificationPending,
                        t.AssignedTeacherID,
                        t.ImageUrls
                    });

                var tasksWaitingVerification = await tasksQuery
                    .Where(t => t.VerificationPending == true && t.TaskVerified == false)
                    .ToListAsync();

                var tasksVerified = await tasksQuery
                    .Where(t => t.VerificationPending == false && t.TaskVerified == true)
                    .ToListAsync();

                var tasksRejected = await tasksQuery
                    .Where(t => t.VerificationPending == false && t.TaskRejected == true)
                    .ToListAsync();

                var result = new
                {
                    tasksWaitingVerification,
                    tasksVerified,
                    tasksRejected
                };

                return Ok(new { message = "SUCCESS: Tasks retrieved successfully.", data = result });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}. Inner Exception: {ex.InnerException?.Message}" });
            }
        }

        // Verify Student Task Completion
        [HttpPut("verify-student-task")]
        public async Task<IActionResult> VerifyStudentTask(string teacherID, string studentID, string taskID)
        {
            if (string.IsNullOrEmpty(teacherID) || string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(taskID))
            {
                return BadRequest(new { error = "UERROR: Invalid teacher, student or task ID. Please provide valid teacher, student and task ID." });
            }

            var studentTaskProgressRecord = await _context.TaskProgresses.FirstOrDefaultAsync(st => st.StudentID == studentID && st.TaskID == taskID);

            if (studentTaskProgressRecord == null)
            {
                return NotFound(new { error = "ERROR: Task completion record not found." });
            }

            if (studentTaskProgressRecord.AssignedTeacherID != teacherID)
            {
                return BadRequest(new { error = "UERROR: You are not authorised to verify this task." });
            }

            if (studentTaskProgressRecord.VerificationPending == false || studentTaskProgressRecord.TaskVerified == true)
            {
                return BadRequest(new { error = "UERROR: Task is not pending verification or has already been verified." });
            }

            try
            {
                var taskObj = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskID == taskID);
                if (taskObj == null)
                {
                    return NotFound(new { error = "ERROR: Task not found." });
                }

                studentTaskProgressRecord.TaskVerified = true;
                studentTaskProgressRecord.VerificationPending = false;
                _context.TaskProgresses.Update(studentTaskProgressRecord);

                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null)
                {
                    return NotFound(new { error = "ERROR: Student not found." });
                }

                var todayDatetimeString = DateTime.Now.ToString("yyyy-MM-dd");
                var existingStudentPointsRecord = await _context.StudentPoints.FirstOrDefaultAsync(sp => sp.StudentID == studentID && sp.TaskID == taskID && sp.DateCompleted == todayDatetimeString);
                if (existingStudentPointsRecord != null)
                {
                    return BadRequest(new { error = "UERROR: Points already awarded for this task." });
                }

                student.CurrentPoints += taskObj.TaskPoints;
                student.TotalPoints += taskObj.TaskPoints;
                _context.Students.Update(student);

                var addStudentPoints = new StudentPoints
                {
                    StudentID = studentID,
                    TaskID = taskID,
                    PointsAwarded = taskObj.TaskPoints,
                    DateCompleted = DateTime.Now.ToString("yyyy-MM-dd")
                };

                _context.StudentPoints.Add(addStudentPoints);

                var associatedQuest = await _context.Quests.FirstOrDefaultAsync(q => q.QuestID == taskObj.AssociatedQuestID);
                if (associatedQuest == null)
                {
                    return NotFound(new { error = "ERROR: Task's associated Quest not found." });
                }

                var studentClassRecord = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);
                if (studentClassRecord == null)
                {
                    return NotFound(new { error = "ERROR: Student's class record not found." });
                }

                var associatedQuestProgress = await _context.QuestProgresses.FirstOrDefaultAsync(qp => qp.QuestID == associatedQuest.QuestID && qp.ClassID == studentClassRecord.ClassID);

                if (associatedQuestProgress != null)
                {
                    if (DateTime.Parse(associatedQuestProgress.DateAssigned) >= DateTime.Now.AddDays(-7))
                    {
                        if (associatedQuestProgress.AmountCompleted + taskObj.QuestContributionAmountOnComplete == associatedQuest.TotalAmountToComplete)
                        {
                            associatedQuestProgress.AmountCompleted = associatedQuest.TotalAmountToComplete;
                            associatedQuestProgress.Completed = true;

                            var existingClassPointsRecord = await _context.ClassPoints.FirstOrDefaultAsync(cp => cp.ClassID == studentClassRecord.ClassID && cp.QuestID == associatedQuest.QuestID && cp.DateCompleted == todayDatetimeString && cp.ContributingStudentID == studentID);

                            if (existingClassPointsRecord == null)
                            {
                                var addClassPoints = new ClassPoints
                                {
                                    ClassID = studentClassRecord.ClassID,
                                    QuestID = associatedQuest.QuestID,
                                    ContributingStudentID = studentID,
                                    PointsAwarded = associatedQuest.QuestPoints,
                                    DateCompleted = DateTime.Now.ToString("yyyy-MM-dd")
                                };

                                _context.ClassPoints.Add(addClassPoints);
                                _context.Quests.Update(associatedQuest);
                            }
                        }
                        else
                        {
                            associatedQuestProgress.AmountCompleted += taskObj.QuestContributionAmountOnComplete;
                            _context.Quests.Update(associatedQuest);
                        }
                    }
                    else
                    {
                        var reccomendResponse = await ReccommendationsManager.RecommendQuestsAsync(_context, associatedQuestProgress.ClassID, 1);

                        _context.QuestProgresses.Remove(associatedQuestProgress);
                        await _context.SaveChangesAsync();

                        if (reccomendResponse != null)
                        {
                            foreach (var quest in reccomendResponse.result)
                            {
                                var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == teacherID);
                                if (assignedTeacher == null)
                                {
                                    return NotFound(new { error = "ERROR: Class's teacher not found" });
                                }

                                var questProgress = new QuestProgress
                                {
                                    QuestID = quest.QuestID,
                                    ClassID = associatedQuestProgress.ClassID,
                                    DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                                    AmountCompleted = taskObj.AssociatedQuestID == quest.QuestID ? taskObj.QuestContributionAmountOnComplete : 0,
                                    Completed = false,
                                    Quest = quest,
                                    AssignedTeacherID = assignedTeacher.TeacherID,
                                    AssignedTeacher = assignedTeacher
                                };

                                _context.QuestProgresses.Add(questProgress);
                            }
                        }
                    }
                }

                var studentInboxMessage = new Inbox
                {
                    UserID = student.StudentID,
                    Message = $"Your recent task: {taskObj.TaskTitle} has been verified. You earned {taskObj.TaskPoints} points.",
                    Date = DateTime.Now.ToString("yyyy-MM-dd")
                };

                _context.Inboxes.Add(studentInboxMessage);

                await _context.SaveChangesAsync();

                var studentUser = _context.Users.FirstOrDefault(u => u.Id == student.StudentID);
                if (studentUser == null)
                {
                    return NotFound(new { error = "ERROR: Student user not found." });
                }
                var studentUsername = studentUser.Name;
                var studentEmail = studentUser.Email;

                var emailVars = new Dictionary<string, string> {
                    { "username", studentUsername },
                    { "taskTitle", taskObj.TaskTitle },
                    { "taskPoints", taskObj.TaskPoints.ToString() }
                };

                await Emailer.SendEmailAsync(studentEmail, $"You've earned {taskObj.TaskPoints} leafs!", "SuccessfulTaskVerification", emailVars);

                return Ok(new { message = "SUCCESS: Task verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}. Inner Exception: {ex.InnerException?.Message}" });
            }
        }

        // Reject Student Task Completion
        [HttpPut("reject-student-task")]
        public async Task<IActionResult> RejectStudentTask(string teacherID, string studentID, string taskID, string rejectionReason)
        {
            if (string.IsNullOrEmpty(teacherID) || string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(taskID))
            {
                return BadRequest(new { error = "UERROR: Invalid teacher, student or task ID. Please provide valid teacher, student and task ID." });
            }

            var studentTaskProgressRecord = await _context.TaskProgresses.FirstOrDefaultAsync(st => st.StudentID == studentID && st.TaskID == taskID);

            if (studentTaskProgressRecord == null)
            {
                return NotFound(new { error = "ERROR: Task completion record not found." });
            }

            if (studentTaskProgressRecord.AssignedTeacherID != teacherID)
            {
                return BadRequest(new { error = "UERROR: You are not authorised to reject this task." });
            }

            if (studentTaskProgressRecord.VerificationPending == false || studentTaskProgressRecord.TaskRejected == true)
            {
                return BadRequest(new { error = "UERROR: Task is not pending verification or has already been rejected." });
            }

            try
            {
                var taskObj = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskID == taskID);
                if (taskObj == null)
                {
                    return NotFound(new { error = "ERROR: Task not found." });
                }

                studentTaskProgressRecord.TaskRejected = true;
                studentTaskProgressRecord.VerificationPending = false;
                _context.TaskProgresses.Update(studentTaskProgressRecord);

                var studentInboxMessage = new Inbox
                {
                    UserID = studentID,
                    Message = $"Your recent task: {taskObj.TaskTitle} has been rejected. Please try again.",
                    Date = DateTime.Now.ToString("yyyy-MM-dd")
                };

                _context.Inboxes.Add(studentInboxMessage);

                await _context.SaveChangesAsync();

                var studentUser = _context.Users.FirstOrDefault(u => u.Id == studentID);
                if (studentUser == null)
                {
                    return NotFound(new { error = "ERROR: Student user not found." });
                }
                var studentUsername = studentUser.Name;
                var studentEmail = studentUser.Email;

                var emailVars = new Dictionary<string, string> {
                    { "username", studentUsername },
                    { "taskTitle", taskObj.TaskTitle },
                    { "rejectionReason", rejectionReason }
                };

                await Emailer.SendEmailAsync(studentEmail, $"Your task: {taskObj.TaskTitle} has been rejected.", "SuccessfulTaskRejection", emailVars);

                return Ok(new { message = "SUCCESS: Task rejected successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}. Inner Exception: {ex.InnerException?.Message}" });
            }
        }

        // Get Class Points
        [HttpGet("get-class-points")]
        public async Task<IActionResult> GetClassPoints(string classID)
        {
            if (string.IsNullOrEmpty(classID))
            {
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try
            {
                // Get today's date and the date 7 days ago
                DateTime today = DateTime.UtcNow.Date;
                DateTime sevenDaysAgo = today.AddDays(-6); // To include today as the 7th day

                // Fetch class points from the last 7 days
                var classPointsRaw = await _context.ClassPoints
                    .Where(cp => cp.ClassID == classID)
                    .OrderBy(cp => cp.DateCompleted) // Sort in ascending order for better mapping
                    .ToListAsync();

                // Initialize a dictionary with 7 days (default value is 0)
                var classPointsDict = Enumerable.Range(0, 7)
                    .ToDictionary(offset => sevenDaysAgo.AddDays(offset), _ => 0);

                // Filter the records in-memory based on DateCompleted string
                foreach (var record in classPointsRaw)
                {
                    if (DateTime.TryParse(record.DateCompleted, out DateTime recordDate) &&
                        recordDate >= sevenDaysAgo && recordDate <= today)
                    {
                        recordDate = recordDate.Date;
                        if (classPointsDict.ContainsKey(recordDate))
                        {
                            classPointsDict[recordDate] += record.PointsAwarded;
                        }
                    }
                }

                // Convert to list format for response
                var classPoints = classPointsDict
                    .OrderBy(entry => entry.Key) // Ensure chronological order
                    .Select(entry => new { date = entry.Key.ToString("yyyy-MM-dd"), points = entry.Value })
                    .ToList();

                return Ok(new { message = "SUCCESS: Class points found.", data = classPoints });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Re-generate Class Quests
        [HttpPost("regenerate-class-quests")]
        public async Task<IActionResult> RegenerateClassQuests([FromForm] string classID, [FromForm] string teacherID)
        {
            if (string.IsNullOrEmpty(classID) || string.IsNullOrEmpty(teacherID))
            {
                return BadRequest(new { error = "UERROR: Invalid class ID or teacher ID. Please provide a valid class ID or teacher ID." });
            }

            var matchedClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classID);
            if (matchedClass == null)
            {
                return NotFound(new { error = "ERROR: Class not found." });
            }

            if (teacherID != matchedClass.TeacherID)
            {
                return BadRequest(new { error = "UERROR: You are not authorised to regenerate quests for this class." });
            }

            try
            {
                var classQuests = _context.QuestProgresses
                    .Where(qp => qp.ClassID == classID)
                    .AsEnumerable()
                    .Where(qp => DateTime.Parse(qp.DateAssigned) >= DateTime.Now.AddDays(-7))
                    .ToList();

                var completedClassQuests = classQuests.Where(qp => qp.Completed == true).ToList();
                var uncompletedClassQuests = classQuests.Where(qp => qp.Completed == false).ToList();

                if (uncompletedClassQuests != null && uncompletedClassQuests.Count > 0)
                {
                    var noOfQuestsToRegenerate = uncompletedClassQuests.Count;
                    _context.QuestProgresses.RemoveRange(uncompletedClassQuests);

                    var reccomendResponse = await ReccommendationsManager.RecommendQuestsAsync(_context, classID, noOfQuestsToRegenerate);

                    var updatedSetOfQuestProgresses = new List<QuestProgress>();
                    var updatedSetOfQuests = new List<dynamic>();

                    foreach (var quest in completedClassQuests)
                    {
                        updatedSetOfQuests.Add(new
                        {
                            quest.Quest.QuestID,
                            quest.Quest.QuestTitle,
                            quest.Quest.QuestDescription,
                            quest.Quest.QuestPoints,
                            quest.Quest.QuestType,
                            quest.Quest.TotalAmountToComplete,
                            AmountCompleted = quest.Quest.TotalAmountToComplete,
                        });
                    }

                    if (reccomendResponse != null)
                    {
                        var reccomendedQuests = reccomendResponse.result;
                        foreach (var quest in reccomendedQuests)
                        {
                            var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == teacherID);
                            if (assignedTeacher == null)
                            {
                                return NotFound(new { error = "ERROR: Class's teacher not found" });
                            }

                            var questProgress = new QuestProgress
                            {
                                QuestID = quest.QuestID,
                                ClassID = classID,
                                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd"),
                                AmountCompleted = 0,
                                Completed = false,
                                Quest = quest,
                                AssignedTeacherID = assignedTeacher.TeacherID,
                                AssignedTeacher = assignedTeacher
                            };

                            updatedSetOfQuestProgresses.Add(questProgress);

                            updatedSetOfQuests.Add(new
                            {
                                quest.QuestID,
                                quest.QuestTitle,
                                quest.QuestDescription,
                                quest.QuestPoints,
                                quest.QuestType,
                                quest.TotalAmountToComplete,
                                AmountCompleted = 0,
                            });

                            _context.QuestProgresses.Add(questProgress);
                        }
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new { message = "SUCCESS: Quests regenerated successfully.", data = updatedSetOfQuests });
                }
                else
                {
                    return BadRequest(new { message = "UERROR: All quests completed. Please wait for the next week to receive new quests" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}. Inner Exception: {ex.InnerException?.Message}" });
            }
        }

        // Send a certificate to class top contributor using Accredible
        [HttpPost("send-certificate")]
        public async Task<IActionResult> SendCertificate(string topContributorName, string topContributorEmail)
        {
            if (string.IsNullOrEmpty(topContributorName) || string.IsNullOrEmpty(topContributorEmail))
            {
                return BadRequest(new { error = "UERROR: Invalid top contributor name or email. Please provide a valid top contributor name or email." });
            }

            try
            {
                var certificateData = new
                {
                    credential = new
                    {
                        recipient = new
                        {
                            name = topContributorName,
                            email = topContributorEmail,
                        },
                        group_id = Environment.GetEnvironmentVariable("ACCREDIBLE_RECYCLIFY_CERTIFICATE_GROUP_ID"),
                        issued_on = DateTime.Now.ToString("yyyy-MM-dd"),
                    }
                };

                var jsonPayload = JsonConvert.SerializeObject(certificateData);

                var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (_httpClient == null)
                {
                    return StatusCode(500, new { error = "ERROR: HttpClient is not initialized." });
                }

                var apiKey = Environment.GetEnvironmentVariable("ACCREDIBLE_API_KEY"); 
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync("https://api.accredible.com/v1/credentials", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { message = "SUCCESS: Certificate sent successfully." });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { error = $"ERROR: An error occurred: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }
    }
}