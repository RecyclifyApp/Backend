using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Teachers {
    [ApiController]
    [Route("api/[controller]")]

    public class TeacherController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;
        // Get Classes
        [HttpGet("get-classes")]
        public async Task<IActionResult> GetClasses(string teacherID) {
            if (string.IsNullOrEmpty(teacherID)) {
                return BadRequest(new{ error = "UERROR: Invalid teacher ID. Please provide a valid teacher ID." });
            }

            try {
                var classes = await _context.Classes
                .Where(c => c.TeacherID == teacherID)
                .OrderBy(c => c.ClassName)
                .ToListAsync();

                if (classes == null || classes.Count == 0) {
                    classes = [];
                    return Ok( new { message = "SUCCESS: No classes found.", data = classes });
                }

                return Ok( new { message = "SUCCESS: Classes found.", data = classes });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Overall Classes Data
        [HttpGet("get-overall-classes-data")]
        public async Task<IActionResult> GetOverallClassesData() {
            try {
                var classes = await _context.Classes.ToListAsync();
                if (classes == null || classes.Count == 0) {
                    classes = [];
                    return Ok( new { message = "SUCCESS: No classes found.", data = classes });
                }

                return Ok( new { message = "SUCCESS: Classes found.", data = classes });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Class
        [HttpGet("get-class")]
        public async Task<IActionResult> GetClass(string classID) {
            if (string.IsNullOrEmpty(classID)) {
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try {
                var classData = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classID);
                if (classData == null) {
                    return Ok( new {message = "SUCCESS: Class not found.", data = classData });
                }

                return Ok(new { message = "SUCCESS: Class found.", data = classData });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Create Class, (Add Class Image later)
        [HttpPost("create-class")]
        public async Task<IActionResult> CreateClass(string className, string classDescription, string teacherID) {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(teacherID)) {
                return BadRequest( new { error = "UERROR: Invalid class details. Please provide valid class details." });
            }

            // Check if class name is an integer (E.g. 101)
            if (!int.TryParse(className, out int intClassName)) {
                return BadRequest(new { error = "UERROR: Class name must be an integer." });
            }

            // Find Teacher
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherID);
            if (teacher == null) {
                return NotFound(new { error = "ERROR: Teacher not found." });
            }

            // Find Class Existance
            var classExist = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == intClassName);
            if (classExist != null) {
                return BadRequest(new { error = "UERROR: Class already exists." });
            }

            try {
                var newClass = new Class {
                    ClassID = Utilities.GenerateUniqueID(),
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
                _context.SaveChanges();

                return Ok(new { message = "SUCCESS: Class created successfully." });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }

        }

        // Delete Class 
        [HttpDelete("delete-class")]
        public async Task<IActionResult> DeleteClass(string classId) {
            if (string.IsNullOrEmpty(classId)) {
                return BadRequest( new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try {
                var classData = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classId);
                if (classData == null)
                {
                    return NotFound(new { error = "ERROR: Class not found." });
                }

                _context.Classes.Remove(classData);
                await _context.SaveChangesAsync();

                return Ok(new { message = "SUCCESS: Class deleted successfully." });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Update Class, Add Class Image later
        [HttpPut("update-class")]
        public async Task<IActionResult> UpdateClass(string classId, string className, string classDescription) {
            if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(className)) {
                return BadRequest( new { error = "UERROR: Invalid class details. Please provide valid class details." });
            }

            if (!int.TryParse(className, out int intClassName)) {
                return BadRequest(new { error = "UERROR: Class name must be an integer." });
            }

            // Check if other class with same name exists
            var classExist = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == intClassName);
            if (classExist != null) {
                return BadRequest(new { error = "UERROR: Class already exists." });
            }

            try {
                var classData = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classId);
                if (classData == null) {
                    return NotFound(new { error = "ERROR: Class not found." });
                }

                classData.ClassName = intClassName;
                classData.ClassDescription = classDescription;
                _context.Classes.Update(classData);
                await _context.SaveChangesAsync();

                return Ok(new { message = "SUCCESS: Class updated successfully." });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Student
        [HttpGet("get-students")]
        public async Task<IActionResult> GetStudents([FromQuery] string classId) {
            if (string.IsNullOrEmpty(classId)){
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try {
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

                if (students == null || students.Count == 0){
                    return Ok(new { message = "SUCCESS: No students found.", data = new List<object>() });
                }

                return Ok(new { message = "SUCCESS: Students retrieved", data = students });
            }
            catch (Exception ex){
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Delete Student
        [HttpDelete("delete-student")]
        public async Task<IActionResult> DeleteStudent(string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest( new { error = "UERROR: Invalid student ID. Please provide a valid student ID." });
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
            if (student == null) {
                return NotFound( new { error = "ERROR: Student not found." });
            }

            try {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return Ok( new { message = "SUCCESS: Student deleted successfully." });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Update Student
        [HttpPut("update-student")]
        public async Task<IActionResult> UpdateStudent(string studentID, string studentName, string studentEmail) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentEmail)) {
                return BadRequest( new { error = "UERROR: Invalid student details. Please provide valid student details." });
            }

            // Find student and student user details
            var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentID == studentID);

            // Collated if clause to check student and student user details
            if (student == null || student.User == null) {
                return NotFound( new { error = "ERROR: Student not found." });
            }

            try {
                student.User.Name = studentName;
                student.User.Email = studentEmail;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();

                return Ok( new { message = "SUCCESS: Student updated successfully." });
            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Get Tasks Waiting for Verification
        [HttpGet("get-tasks-waiting-verification")]
        public async Task<IActionResult> GetTasksWaitingVerification(string teacherID) {
            if (string.IsNullOrEmpty(teacherID)) {
                return BadRequest( new { error = "UERROR: Invalid Teacher ID. Please provide a valid Teacher ID." });
            }

            try {
                var tasksProgressRecords = await _context.TaskProgresses
                .Where(t => t.AssignedTeacherID == teacherID && t.VerificationPending == true && t.TaskVerified == false)
                .ToListAsync();

                if (tasksProgressRecords == null || tasksProgressRecords.Count == 0) {
                    tasksProgressRecords = [];
                    return Ok( new { message = "SUCCESS: No tasks found.", data = tasksProgressRecords });
                }

                return Ok( new { message = "SUCCESS: Tasks retrieved successfully.", data = tasksProgressRecords });

            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }

        // Verify Student Task Completion
        [HttpPut("verify-student-task")]
        public async Task<IActionResult> VerifyStudentTask(string studentID, string taskID) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(taskID)) {
                return BadRequest( new { error = "UERROR: Invalid student or task ID. Please provide valid student and task ID." });
            }

            var studentTaskProgressRecord = await _context.TaskProgresses.FirstOrDefaultAsync(st => st.StudentID == studentID && st.TaskID == taskID);

            if (studentTaskProgressRecord == null) {
                return NotFound( new { error = "ERROR: Task completion record not found." });
            }

            if (studentTaskProgressRecord.VerificationPending == false || studentTaskProgressRecord.TaskVerified == true) {
                return BadRequest( new { error = "UERROR: Task is not pending verification or has already been verified." });
            }

            try {
                var taskObj = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskID == taskID);
                if (taskObj == null) {
                    return NotFound(new { error = "ERROR: Task not found." });
                }

                studentTaskProgressRecord.TaskVerified = true;
                studentTaskProgressRecord.VerificationPending = false;
                _context.TaskProgresses.Update(studentTaskProgressRecord);

                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "ERROR: Student not found." });
                }

                student.CurrentPoints += taskObj.TaskPoints;
                student.TotalPoints += taskObj.TaskPoints;
                _context.Students.Update(student);

                var addStudentPoints = new StudentPoints {
                    StudentID = studentID,
                    TaskID = taskID,
                    PointsAwarded = taskObj.TaskPoints,
                    DateCompleted = DateTime.Now.ToString("yyyy-MM-dd")
                };

                _context.StudentPoints.Add(addStudentPoints);

                var associatedQuest = await _context.Quests.FirstOrDefaultAsync(q => q.QuestID == taskObj.AssociatedQuestID);
                if (associatedQuest == null) {
                    return NotFound(new { error = "ERROR: Task's associated Quest not found." });
                }

                var studentClassRecord = await _context.ClassStudents.FirstOrDefaultAsync(cs => cs.StudentID == studentID);
                if (studentClassRecord == null) {
                    return NotFound(new { error = "ERROR: Student's class record not found." });
                }

                var associatedQuestProgress = await _context.QuestProgresses.FirstOrDefaultAsync(qp => qp.QuestID == associatedQuest.QuestID && qp.ClassID == studentClassRecord.ClassID);

                if (associatedQuestProgress != null) {
                    if (associatedQuestProgress.AmountCompleted + taskObj.QuestContributionAmountOnComplete == associatedQuest.TotalAmountToComplete) {
                        associatedQuestProgress.AmountCompleted = associatedQuest.TotalAmountToComplete;
                        associatedQuestProgress.Completed = true;

                        var addClassPoints = new ClassPoints {
                            ClassID = studentClassRecord.ClassID,
                            QuestID = associatedQuest.QuestID,
                            PointsAwarded = associatedQuest.QuestPoints,
                            DateCompleted = DateTime.Now.ToString("yyyy-MM-dd")
                        };

                        _context.ClassPoints.Add(addClassPoints);
                        _context.Quests.Update(associatedQuest);
                    } else {
                        associatedQuestProgress.AmountCompleted += taskObj.QuestContributionAmountOnComplete;
                        _context.Quests.Update(associatedQuest);
                    }
                }

                var studentInboxMessage = new Inbox {
                    UserID = student.StudentID,
                    Message = $"Your recent task: {taskObj.TaskTitle} has been verified. You earned {taskObj.TaskPoints} points.",
                };

                _context.Inboxes.Add(studentInboxMessage);

                await _context.SaveChangesAsync();

                var studentUser = _context.Users.FirstOrDefault(u => u.Id == student.StudentID);
                if (studentUser == null) {
                    return NotFound(new { error = "ERROR: Student user not found." });
                }
                var studentUsername = studentUser.Name;
                var studentEmail = studentUser.Email;

                Console.WriteLine($"Student: {studentUsername}, Email: {studentEmail}");

                var emailVars = new Dictionary<string, string> {
                    { "username", studentUsername },
                    { "taskTitle", taskObj.TaskTitle },
                    { "taskPoints", taskObj.TaskPoints.ToString() }
                };

                await Emailer.SendEmailAsync(studentEmail, $"You've earned {taskObj.TaskPoints} leafs!", "SuccessfulTaskVerification", emailVars);

                return Ok( new { message = "SUCCESS: Task verified successfully." });
            } catch (Exception ex) {
                return StatusCode(500, new { error = $"ERROR: An error occurred: {ex.Message}" });
            }
        }
    }
}