using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Filters;
using Backend.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;


namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class UserManagementController(MyDbContext _context) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(new { message = "SUCCESS: Users retrieved", data = users });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "UERROR: Invalid user data" });
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { error = "ERROR: User not found" });
            }

            // Check for duplicate Name
            var duplicateName = await _context.Users
                .Where(u => u.Name == updatedUser.Name && u.Id != id) // Ensure it's not the same user
                .FirstOrDefaultAsync();
            if (duplicateName != null)
            {
                return BadRequest(new { error = "ERROR: Name is already taken" });
            }

            // Check for duplicate Email
            var duplicateEmail = await _context.Users
                .Where(u => u.Email == updatedUser.Email && u.Id != id) // Ensure it's not the same user
                .FirstOrDefaultAsync();
            if (duplicateEmail != null)
            {
                return BadRequest(new { error = "ERROR: Email is already taken" });
            }

            // Check for duplicate Contact Number
            var duplicatePhone = await _context.Users
                .Where(u => u.ContactNumber == updatedUser.ContactNumber && u.Id != id) // Ensure it's not the same user
                .FirstOrDefaultAsync();
            if (duplicatePhone != null)
            {
                return BadRequest(new { error = "ERROR: Contact Number is already taken" });
            }

            // Proceed with updating user
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.ContactNumber = updatedUser.ContactNumber;
            existingUser.UserRole = updatedUser.UserRole;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "SUCCESS: User updated", data = existingUser });
            }
            catch
            {
                return StatusCode(500, new { error = "ERROR: An error occurred while updating the user" });
            }
        }

        [HttpPost("CreateTeacherAccount")]
        [Authorize]
        public async Task<IActionResult> CreateTeacherAccount([FromBody] CreateTeacherAccountRequest request)
        {
            if (request.UserRole?.ToLower() != "teacher")
            {
                return BadRequest(new { error = "ERROR: Only teacher accounts can be created." });
            }

            string email = request.Email;
            var keyValuePairs = new List<Dictionary<string, object>> {
                new Dictionary<string, object> {
                    { "Name", request.Name },
                    { "FName", request.FName},
                    { "LName", request.LName},
                    { "Email", email },
                    { "Password", request.Password },
                    { "ContactNumber", request.ContactNumber },
                    { "UserRole", request.UserRole },
                }
            };

            try
            {
                await DatabaseManager.CreateUserRecords(_context, request.UserRole, keyValuePairs);

                var user = _context.Users.SingleOrDefault(u => u.Email == email);
                if (user == null)
                {
                    return BadRequest(new { error = "ERROR: User creation failed." });
                }

                // Generate 6-digit code
                var code = Utilities.GenerateRandomInt(111111, 999999).ToString();
                var expiry = DateTime.UtcNow.AddMinutes(15).ToString("o"); // ISO 8601 format

                // Store in database
                user.EmailVerificationToken = code;
                user.EmailVerificationTokenExpiry = expiry;
                _context.SaveChanges();

                var emailVars = new Dictionary<string, string> {
                    { "username", user.Name },
                    { "emailVerificationToken", code }
                };

                var Emailer = new Emailer(_context);
                var result = await Emailer.SendEmailAsync(
                    user.Email,
                    "Welcome to Recyclify",
                    "WelcomeEmail",
                    emailVars
                );

                string token = CreateToken(user);

                Logger.Log($"[SUCCESS] ADMIN CREATETEACHERACCOUNT: User {user.Id} created.");

                await CreateClass(request.classNumber.ToString(), request.classDescription, user.Id);

                return Ok(new
                {
                    message = "SUCCESS: Account created successfully.",
                    token,
                    user = new
                    {
                        user.Id,
                        user.Name,
                        user.FName,
                        user.LName,
                        user.Email,
                        user.UserRole
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "UERROR: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "ERROR: An error occurred while creating the account.", details = ex.Message });
            }
        }

        private string CreateToken(User user)
        {
            string? secret = Environment.GetEnvironmentVariable("JWT_KEY");
            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("JWT secret is missing.");
            }

            // Validate user properties
            if (user.Id == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserRole))
            {
                throw new Exception("User properties are missing or invalid.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserRole)
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<string> CreateClass(string className, string classDescription, string teacherID)
        {
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(classDescription) || string.IsNullOrEmpty(teacherID))
            {
                return "UERROR: Invalid class details. Please provide valid class details.";
            }

            // Check if class name is an integer (E.g. 101)
            if (!int.TryParse(className, out int intClassName))
            {
                return "UERROR: Class name must be an integer.";
            }

            // Find Teacher
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherID == teacherID);
            if (teacher == null)
            {
                return "ERROR: Teacher not found.";
            }

            // Find Class Existence
            var classExist = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == intClassName);
            if (classExist != null)
            {
                return "UERROR: Class already exists.";
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
                    WeeklyClassPoints = new List<WeeklyClassPoints>(),
                    TeacherID = teacherID,
                    Teacher = teacher,
                    JoinCode = Utilities.GenerateRandomInt(100000, 999999)
                };

                _context.Classes.Add(newClass);

                var recommendResponse = await ReccommendationsManager.RecommendQuestsAsync(_context, classID, 3);

                if (recommendResponse != null)
                {
                    foreach (var quest in recommendResponse.result)
                    {
                        var assignedTeacher = _context.Teachers.FirstOrDefault(t => t.TeacherID == teacherID);
                        if (assignedTeacher == null)
                        {
                            return "ERROR: Class's teacher not found";
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

                await _context.SaveChangesAsync();

                return "SUCCESS: Class created successfully.";

            }
            catch (Exception ex)
            {
                return $"ERROR: An error occurred: {ex.Message}";
            }
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // Find the user first
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                // User not found, return 404
                return NotFound(new { error = "ERROR: User not found" });
            }

            // Check if the user is a teacher
            if (user.UserRole == "teacher")
            {
                try
                {
                    // Fetch all related classes where the teacher is assigned
                    var relatedClasses = await _context.Classes.Where(c => c.TeacherID == id).ToListAsync();
                    if (relatedClasses.Any())
                    {
                        // Remove all related classes
                        _context.Classes.RemoveRange(relatedClasses);

                        // Save changes to delete classes
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log error if deleting related classes fails
                    return StatusCode(500, new { error = "ERROR: Failed to delete related classes.", details = ex.Message });
                }
            }

            // Now delete the user
            _context.Users.Remove(user);

            try
            {
                // Save changes to delete the user
                await _context.SaveChangesAsync();
                return Ok(new { message = "SUCCESS: User deleted successfully" });
            }
            catch (Exception ex)
            {
                // Log error if deleting the user fails
                return StatusCode(500, new { error = "ERROR: An error occurred while deleting the user.", details = ex.Message });
            }
        }

    }

    public class CreateTeacherAccountRequest
    {
        public required string Name { get; set; }
        public required string FName { get; set; }
        public required string LName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ContactNumber { get; set; }
        public required string UserRole { get; set; }
        public required int classNumber { get; set; }
        public required string classDescription { get; set; }
    }
}