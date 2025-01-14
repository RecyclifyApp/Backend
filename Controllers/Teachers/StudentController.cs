using Backend.Models;
using Backend.Services;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Teachers {
    [ApiController]
    [Route("[controller]")]

    public class StudentController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        // Get Student
        [HttpGet("get-student")]
        public async Task<IActionResult> GetStudent(string classId) {
            if (string.IsNullOrEmpty(classId)) {
                return BadRequest("Invalid class ID. Please provide a valid class ID.");
            }

            try {
                var students = await _context.Students
                .Where(s => s.ClassID == classId)
                .Include(s => s.User)
                .ToListAsync();

                if (students == null || students.Count == 0) {
                    return NotFound("No students found in this class.");
                }

                return Ok(students);

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Delete Student
        [HttpDelete("delete-student")]
        public async Task<IActionResult> DeleteStudent(string studentID) {
            if (string.IsNullOrEmpty(studentID)) {
                return BadRequest("Invalid student ID. Please provide a valid student ID.");
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == studentID);
            if (student == null) {
                return NotFound("Student not found.");
            }

            try {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                return Ok("Student deleted successfully.");
            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Update Student
        // [HttpPut("update-student")]
        // public async Task<IActionResult> UpdateStudent(string studentID, string studentName, string studentEmail, string studentContactNumber, string studentAvatar) {
        //     if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentEmail) || string.IsNullOrEmpty(studentContactNumber) || string.IsNullOrEmpty(studentAvatar)) {
        //         return BadRequest("Invalid student details. Please provide valid student details.");
        //     }

        //     var student = await _context.Students
        //     .Include(s => s.User) 
        //     .FirstOrDefaultAsync(s => s.StudentID == studentID);

        //     if (student == null) {
        //         return NotFound("Student not found.");
        //     }

        //     try {
        //         if (student.User == null) {
        //             return NotFound("Associated user for this student not found.");
        //         }

        //         student.User.Name = studentName;
        //         student.User.Email = studentEmail;
        //         student.User.ContactNumber = studentContactNumber;
        //         student.User.Avatar = studentAvatar;

        //         _context.Students.Update(student);
        //         await _context.SaveChangesAsync();

        //         return Ok("Student updated successfully.");
        //     }
        //     catch (Exception ex) {
        //         return StatusCode(500, $"An error occurred: {ex.Message}");
        //     }
        // }
    }
}