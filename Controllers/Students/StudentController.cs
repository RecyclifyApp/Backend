using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class studentController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        [HttpGet("get-student")]
        public IActionResult GetStudent([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "Student not found" });
                }

                var classStudents = _context.Students.Where(s => s.ClassID == matchedStudent.ClassID).ToList() ?? new List<Student>();
                var classStudentsRanked = classStudents.OrderByDescending(s => s.TotalPoints).ToList();
                matchedStudent.LeagueRank = classStudentsRanked.FindIndex(s => s.StudentID == studentID) + 1;

                _context.SaveChanges();

                return Ok(new { message = "SUCCESS: Student details retrieved", data = matchedStudent });
            }
        }

        [HttpGet("get-all-students")]
        public async Task<IActionResult> GetAllStudents([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "Student ID is required" });
            }

            var studentClass = _context.Students
                .FirstOrDefault(s => s.StudentID == studentID)?.ClassID;

            if (studentClass == null) {
                return NotFound(new { error = "Student's class not found" });
            }

            
            var allStudents = await _context.Students
                .Where(s => s.ClassID == studentClass)
                .OrderByDescending(s => s.TotalPoints)
                .Select(s => new {
                    s.StudentID,
                    s.ClassID,
                    s.ParentID,
                    s.League,
                    s.LeagueRank,
                    s.CurrentPoints,
                    s.TotalPoints,
                    s.UserID,
                    s.TaskLastSet,
                    s.Streak,
                    s.LastClaimedStreak,
                    Name = _context.Users.FirstOrDefault(u => u.Id == s.StudentID).Name
                }).ToListAsync();

            return Ok(new { message = "SUCCESS: All students retrieved", data = allStudents });
        }


        [HttpGet("get-student-tasks")]
        public async Task<IActionResult> GetStudentTasks([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "Student not found" });
                } else {
                    var studentTaskProgresses = _context.TaskProgresses.Where(tp => tp.StudentID == matchedStudent.StudentID).ToList();
                    var todayTaskProgresses = studentTaskProgresses.Where(tp => DateTime.Parse(tp.DateAssigned) == DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"))).ToList();
                    
                    if (todayTaskProgresses.Count == 0) {
                        var allTasks = _context.Tasks.ToList();
                        var randomTasks = allTasks.OrderBy(t => Utilities.GenerateUniqueID()).Take(3).ToList();
                        foreach (var task in randomTasks) {
                            var studentClass = _context.Classes.FirstOrDefault(c => c.ClassID == matchedStudent.ClassID);
                            if (studentClass == null) {
                                return NotFound(new { error = "Student's class not found" });
                            }

                            var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == studentClass.TeacherID);
                            if (assignedTeacher == null) {
                                return NotFound(new { error = "Class's teacher not found" });
                            }

                            var taskProgress = new TaskProgress {
                                Task = task,
                                AssignedTeacher = assignedTeacher,
                                Student = matchedStudent,
                                TaskID = task.TaskID,
                                StudentID = matchedStudent.StudentID,
                                TaskVerified = false,
                                VerificationPending = false,
                                AssignedTeacherID = assignedTeacher.TeacherID,
                                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd")
                            };

                            _context.TaskProgresses.Add(taskProgress);
                            _context.SaveChanges();
                        }
                        return Ok(new { message = "SUCCESS: Student tasks assigned", data = randomTasks });       
                    } else {
                        var studentTasks = new List<dynamic>();
                        foreach (var task in todayTaskProgresses) {
                            var foundTask = _context.Tasks.FirstOrDefault(t => t.TaskID == task.TaskID);
                            if (foundTask != null) {
                                var taskProgress = await _context.TaskProgresses.Where(tp => tp.TaskID == foundTask.TaskID && tp.StudentID == matchedStudent.StudentID).ToListAsync();
                                var selectedTaskProgress = taskProgress.OrderByDescending(tp => tp.DateAssigned).FirstOrDefault();

                                studentTasks.Add(new {
                                    foundTask.TaskID,
                                    foundTask.TaskTitle,
                                    foundTask.TaskDescription,
                                    foundTask.TaskPoints,
                                    selectedTaskProgress.VerificationPending,
                                    selectedTaskProgress.TaskVerified,
                                });
                            }
                        }
                        return Ok(new { message = "SUCCESS: Student tasks retrieved", data = studentTasks });
                    }
                }
            }
        }

        [HttpGet("get-student-chart-statistics")]
        public IActionResult GetStudentChartStatistics([FromQuery] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "Student ID is required" });
            } else {
                var matchedStudent = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (matchedStudent == null) {
                    return NotFound(new { error = "Student not found" });
                } else {
                    var studentPointRecords = _context.StudentPoints.Where(sp => sp.StudentID == studentID).ToList();
                    var studentPointsObj = new Dictionary<string, int> {
                        { "Monday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Monday").Sum(sp => sp.PointsAwarded) },
                        { "Tuesday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Tuesday").Sum(sp => sp.PointsAwarded) },
                        { "Wednesday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Wednesday").Sum(sp => sp.PointsAwarded) },
                        { "Thursday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Thursday").Sum(sp => sp.PointsAwarded) },
                        { "Friday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Friday").Sum(sp => sp.PointsAwarded) },
                        { "Saturday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Saturday").Sum(sp => sp.PointsAwarded) },
                        { "Sunday", studentPointRecords.Where(sp => DateTime.Parse(sp.DateCompleted).DayOfWeek.ToString() == "Sunday").Sum(sp => sp.PointsAwarded) }
                    };

                    return Ok(new { message = "SUCCESS: Student chart statistics retrieved", data = studentPointsObj });
                }
            }
        }

        [HttpPost("submit-task")]
        public async Task<IActionResult> SubmitTask([FromForm] IFormFile file, [FromForm] string taskID, [FromForm] string studentID) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "No file uploaded" });
            } 

            if (string.IsNullOrEmpty(taskID) || string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "Task ID and Student ID are required" });
            } else {
                try {
                    var task = _context.Tasks.FirstOrDefault(t => t.TaskID == taskID);
                    if (task == null) {
                        return NotFound(new { error = "Associated task not found" });
                    }

                    var student = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                    if (student == null) {
                        return NotFound(new { error = "Student not found" });
                    }

                    var studentClass = _context.Classes.FirstOrDefault(c => c.ClassID == student.ClassID);
                    if (studentClass == null) {
                        return NotFound(new { error = "Student's class not found" });
                    }

                    var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == studentClass.TeacherID);
                    if (assignedTeacher == null) {
                        return NotFound(new { error = "Class's teacher not found" });
                    }

                    var taskProgress = await _context.TaskProgresses.Where(tp => tp.TaskID == taskID && tp.StudentID == studentID).ToListAsync();
                    var selectedTaskProgress = taskProgress.OrderByDescending(tp => tp.DateAssigned).FirstOrDefault();
                    if (selectedTaskProgress == null) {
                        return NotFound(new { error = "Task progress not found", data = taskProgress });
                    }

                    try {
                        await AssetsManager.UploadFileAsync(file);
                        selectedTaskProgress.ImageUrls = await AssetsManager.GetFileUrlAsync(file.FileName);
                        selectedTaskProgress.VerificationPending = true;

                        try {
                            await _context.SaveChangesAsync();
                            return Ok(new { message = "Task submitted successfully" });
                        } catch (Exception ex) {
                            return StatusCode(500, new { error = "Failed to save changes: " + ex.Message });
                        }
                    } catch (Exception ex) {
                        var innerException = ex.InnerException?.Message;
                        return StatusCode(500, new { error = "Failed to upload image: " + innerException });
                    }
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }

        [HttpPost("award-gift")]
        public IActionResult AwardGift([FromBody] string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest(new { error = "Student ID is required" });
            } else {
                var student = _context.Students.FirstOrDefault(s => s.StudentID == studentID);
                if (student == null) {
                    return NotFound(new { error = "Student not found" });
                }

                var randomPoints = Utilities.GenerateRandomInt(10, 100);

                try {
                    student.CurrentPoints += randomPoints;
                    student.TotalPoints += randomPoints;
                    student.LastClaimedStreak = DateTime.Now.ToString("yyyy-MM-dd");
                    _context.SaveChanges();
                    return Ok(new { message = "Gift awarded successfully", data = new { pointsAwarded = randomPoints, currentPoints = student.CurrentPoints } });
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }
            
        [HttpPost("recognise-image")]
        public async Task<IActionResult> RecogniseImage([FromForm] IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "No file uploaded" });
            } else {
                try {
                    var recognitionResult = await CompVision.Recognise(file);
                    return Ok(recognitionResult);
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex });
                }
            }
        }
    }
}