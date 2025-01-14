using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        [HttpPost("submit-task")]
        public async Task<IActionResult> SubmitTask([FromForm] IFormFile file, [FromForm] string taskID, [FromForm] string taskTitle, [FromForm] string taskDescription, [FromForm] int taskPoints) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "No file uploaded" });
            } else {
                try {
                    var task = new Models.Task {
                        TaskID = taskID,
                        TaskTitle = taskTitle,
                        TaskDescription = taskDescription,
                        TaskPoints = taskPoints
                    };

                    var student = _context.Students.FirstOrDefault(s => s.StudentID == "3f9056b0-06e1-487a-8901-586bafd1e492"); // Student record 1
                    if (student == null) {
                        return NotFound(new { error = "Student not found" });
                    }

                    var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == "c1f76fc4-c99b-4517-9eac-c5ae54bb8808"); // Teacher 1
                    if (assignedTeacher == null) {
                        return NotFound(new { error = "Teacher not found" });
                    }

                    var taskProgress = new TaskProgress {
                        Task = task,
                        AssignedTeacher = assignedTeacher,
                        Student = student,
                        TaskID = task.TaskID,
                        StudentID = student.StudentID,
                        Progress = "Completed",
                        TaskVerified = false,
                        AssignedTeacherID = assignedTeacher.TeacherID
                    };

                    try {
                        await AssetsManager.UploadFileAsync(file);
                        taskProgress.ImageUrls = await AssetsManager.GetFileUrlAsync(file.FileName);

                        _context.TaskProgresses.Add(taskProgress);
                        await _context.SaveChangesAsync();

                        return Ok(new { message = "Task submitted successfully" });
                    } catch (Exception ex) {
                        return StatusCode(500, new { error = "Failed to upload image file, " + ex.Message });
                    }
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }
    }
}