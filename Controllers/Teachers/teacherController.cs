using Backend.Models;
using Backend.Services;
using Google.Api;
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
                return BadRequest("Invalid teacher ID. Please provide a valid teacher ID.");
            }

            try {
                var classes = await _context.Classes
                .Where(c => c.TeacherID == teacherID)
                .OrderBy(c => c.ClassName)
                .ToListAsync();

                if (classes == null || classes.Count == 0) {
                    return Ok("No classes found for the provided teacher ID.");
                }

                return Ok(classes);

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Get Class
        [HttpGet("get-class")]
        public async Task<IActionResult> GetClass(string classID) {
            if (string.IsNullOrEmpty(classID)) {
                return BadRequest("Invalid class ID. Please provide a valid class ID.");
            }

            try {
                var result = await _context.Classes.FirstOrDefaultAsync(c => c.ClassID == classID);
                if (result == null) {
                    return NotFound("Class not found.");
                }

                return Ok(result);

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Create Class
        [HttpPost("create-class")]
        public async Task<IActionResult> CreateClass(string className, string classDescription, string teacherID) {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(teacherID) || string.IsNullOrEmpty(classDescription)) {
                return BadRequest("Invalid class details. Please provide valid class details and teacher ID.");
            }

            if (!int.TryParse(className, out int intClassName)) {
                return BadRequest("Class name must be an integer.");
            }

            // Find Teacher
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherID);
            if (teacher == null) {
                return NotFound("Teacher not found.");
            }

            // Find Class Existance
            var classExist = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == intClassName && c.TeacherID == teacherID);
            if (classExist != null) {
                return BadRequest("Class already exists.");
            }

            try {
                var newClass = new Class {
                    ClassID = Guid.NewGuid().ToString(),
                    ClassName = intClassName,
                    ClassDescription = classDescription,
                    ClassImage = "",
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
                    return NotFound("Class not found.");
                }

                _context.Classes.Remove(result);
                await _context.SaveChangesAsync();

                return Ok("Class deleted successfully.");

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Update Class, Add Class Image later
        [HttpPut("update-class")]
        public async Task<IActionResult> UpdateClass(string classId, string className, string classDescription ) {
            if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(className)) {
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
                _context.Classes.Update(result);
                await _context.SaveChangesAsync();

                return Ok("Class updated successfully.");

            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

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

        // Update Student
        [HttpPut("update-student")]
        public async Task<IActionResult> UpdateStudent(string studentID, string studentName, string studentEmail) {
            if (string.IsNullOrEmpty(studentID) || string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(studentEmail)) {
                return BadRequest("Invalid student details. Please provide valid student details.");
            }

            var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StudentID == studentID);

            // Collate all in one if clause
            if (student == null || student.User == null) {
                return NotFound("Student not found.");
            }

            try {
                student.User.Name = studentName;
                student.User.Email = studentEmail;
                _context.Students.Update(student);
                await _context.SaveChangesAsync();

                return Ok("Student updated successfully.");
            }
            catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}