using Backend.Models;
using Backend.Services;
using Google.Rpc;
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
                    Teacher = teacher
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
            if (string.IsNullOrEmpty(classId)) {
                return BadRequest(new { error = "UERROR: Invalid class ID. Please provide a valid class ID." });
            }

            try {
                var students = await _context.ClassStudents
                    .Where(cs => cs.ClassID == classId)
                    .Join(_context.Students, cs => cs.StudentID, s => s.StudentID, (cs, s) => s)
                    .Include(s => s.User)
                    .ToListAsync(); 

                return Ok(new { message = "SUCCESS: Students retrieved", data = students });
            } catch (Exception ex) {
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
    }
}