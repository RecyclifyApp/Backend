using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;

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

        [HttpGet("get-student-tasks")]
        public IActionResult GetStudentTasks([FromQuery] string studentID) {
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
                                VerificationPending = true,
                                AssignedTeacherID = assignedTeacher.TeacherID,
                                DateAssigned = DateTime.Now.ToString("yyyy-MM-dd")
                            };

                            _context.TaskProgresses.Add(taskProgress);
                            _context.SaveChanges();
                        }
                        return Ok(new { message = "SUCCESS: Student tasks assigned", data = randomTasks });       
                    } else {
                        var studentTasks = new List<Models.Task>();
                        foreach (var task in todayTaskProgresses) {
                            var foundTask = _context.Tasks.FirstOrDefault(t => t.TaskID == task.TaskID);
                            if (foundTask != null) {
                                studentTasks.Add(foundTask);
                            }
                        }
                        return Ok(new { message = "SUCCESS: Student tasks retrieved", data = studentTasks });
                    }
                }
            }
        }

        [HttpPost("submit-task")]
        public async Task<IActionResult> SubmitTask([FromForm] IFormFile file, [FromForm] string taskID, [FromForm] string studentID) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "No file uploaded" });
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

                    var taskProgress = new TaskProgress {
                        Task = task,
                        AssignedTeacher = assignedTeacher,
                        Student = student,
                        TaskID = task.TaskID,
                        StudentID = student.StudentID,
                        TaskVerified = false,
                        VerificationPending = true,
                        AssignedTeacherID = assignedTeacher.TeacherID
                    };

                    try {
                        await AssetsManager.UploadFileAsync(file);
                        taskProgress.ImageUrls = await AssetsManager.GetFileUrlAsync(file.FileName);

                        try {
                            _context.TaskProgresses.Add(taskProgress);
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