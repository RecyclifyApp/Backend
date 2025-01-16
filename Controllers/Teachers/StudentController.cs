using Backend.Models;
using Backend.Services;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Teachers {
    [ApiController]
    [Route("api/[controller]")]

    public class StudentController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;

        // Get Student
        [HttpGet("get-students")]
        public async Task<IActionResult> GetStudent(string classId) {
            if (string.IsNullOrEmpty(classId)) {
                return BadRequest("Invalid class ID. Please provide a valid class ID.");
            }

            try {
                var students = await _context.Students
                .Where(s => s.ClassID == classId)
                .Include(s => s.User)
                .ToListAsync();

                // Return a good response even if there are no students found in the class
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
    }
}