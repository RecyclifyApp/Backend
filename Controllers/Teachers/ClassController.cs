using Backend.Models;
using Backend.Services;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.Teachers {
    [ApiController]
    [Route("[controller]")]

    public class ClassController(MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;
        // Get Class 
        [HttpGet("get-class")]
        public async Task<IActionResult> GetClass(string teacherID) {
            if (string.IsNullOrEmpty(teacherID)) {
                return BadRequest("Invalid teacher ID. Please provide a valid teacher ID.");
            }

            try {
                var classes = await _context.Classes
                .Where(c => c.TeacherID == teacherID)
                .ToListAsync();

                if (classes == null || classes.Count == 0) {
                    return NotFound("No classes found for the provided teacher ID.");
                }

                return Ok(classes);

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Create Class
        [HttpPost("create-class")]
        public async Task<IActionResult> CreateClass(string className, string classDescription, string classImage, string teacherID) {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(teacherID) || string.IsNullOrEmpty(classImage) || string.IsNullOrEmpty(classDescription)) {
                return BadRequest("Invalid class details or teacher ID. Please provide valid class details and teacher ID.");
            }

            if (!int.TryParse(className, out int intClassName)) {
                return BadRequest("Class name must be an integer.");
            }

            // Find Teacher
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherID);
            if (teacher == null) {
                return NotFound("Teacher not found.");
            }

            try {
                var newClass = new Class {
                    ClassID = Guid.NewGuid().ToString(),
                    ClassName = intClassName,
                    ClassDescription = classDescription,
                    ClassImage = classImage,
                    ClassPoints = 0,
                    WeeklyClassPoints = [],
                    TeacherID = teacherID,
                    Teacher = teacher
                };

                _context.Classes.Add(newClass);
                _context.SaveChanges();

                return Ok("Class created successfully.");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}.");
            }

        }

        // Delete Class 
        [HttpDelete("delete-class")]
        public async Task<IActionResult> DeleteClass(string classId) {
            if (string.IsNullOrEmpty(classId)) {
                return BadRequest("Invalid class ID. Please provide a valid class ID.");
            }

            try {
                var result = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classId);
                if (result == null) {
                    return Ok("Class not found.");
                }

                _context.Classes.Remove(result);
                await _context.SaveChangesAsync();

                return Ok("Class deleted successfully.");

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Update Class 
        [HttpPut("update-class")]
        public async Task<IActionResult> UpdateClass(string classId, string className, string classDescription, string classImage) {
            if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(classImage) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(className)) {
                return BadRequest("Invalid class details. Please provide valid class details.");
            }

            if (!int.TryParse(className, out int intClassName)) {
                return BadRequest("Class name must be an integer.");
            }

            try {
                var result = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classId);
                if (result == null) {
                    return Ok("Class not found.");
                }

                result.ClassName = intClassName;
                result.ClassDescription = classDescription;
                result.ClassImage = classImage;
                _context.Classes.Update(result);
                await _context.SaveChangesAsync();

                return Ok("Class updated successfully.");

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}