using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Models;
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers.Identity {
    [ApiController]
    [Route("[controller]")]
    public class IdentityController (MyDbContext context, IConfiguration configuration) : ControllerBase {
        private readonly MyDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        private string CreateToken(User user) {
            string? secret = Environment.GetEnvironmentVariable("JWT_KEY");
            if (string.IsNullOrEmpty(secret)) {
                throw new Exception("JWT secret is missing.");
            }

                // Validate user properties
            if (user.Id == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserRole)) {
                throw new Exception("User properties are missing or invalid.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor {
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

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request) {
            var user = _context.Users.SingleOrDefault(u =>
                (u.Email == request.Identifier || u.Name == request.Identifier) &&
                u.Password == Utilities.HashString(request.Password));

            if (user == null) {
                return Unauthorized(new { message = "Invalid login credentials." });
            }

            // Generate JWT Token
            string token = CreateToken(user);

            // Return the token and user details
            return Ok(new {
                message = "Login successful",
                token,
                user = new {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.UserRole
                }
            });
        }

        [HttpPost("createAccount")]
        public IActionResult CreateAccount([FromBody] CreateAccountRequest request)
        {
            var keyValuePairs = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Name", request.Name },
                    { "Email", request.Email },
                    { "Password", request.Password },
                    { "ContactNumber", request.ContactNumber },
                    { "UserRole", request.UserRole },
                    { "Avatar", request.Avatar }
                }
            };

            if (request.UserRole == "parent") {
                if (string.IsNullOrEmpty(request.StudentID)) {
                    return BadRequest(new { Error = "StudentID cannot be empty for a user with the 'parent' role." });
                }
                keyValuePairs[0].Add("StudentID", request.StudentID);
            }

            try {
                DatabaseManager.CreateUserRecords(_context, request.UserRole, keyValuePairs);
                return Ok("Account created successfully.");
            } catch (ArgumentException ex) {
                return BadRequest(new { message = ex.Message });
            } catch (Exception ex) {
                return StatusCode(500, new { message = "An error occurred while creating the account.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult EditAccount(string id, [FromBody] EditAccountRequest request)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var name = string.IsNullOrWhiteSpace(request.Name) ? user.Name : request.Name.Trim();
                var email = string.IsNullOrWhiteSpace(request.Email) ? user.Email : DatabaseManager.ValidateEmail(request.Email.Trim(), _context);
                var contactNumber = string.IsNullOrWhiteSpace(request.ContactNumber) ? user.ContactNumber : DatabaseManager.ValidateContactNumber(request.ContactNumber.Trim(), _context);

                user.Name = name;
                user.Email = email;
                user.ContactNumber = contactNumber;

                _context.SaveChanges();
                return Ok(new { message = "Account updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the account.", details = ex.Message });
            }
        }

        [HttpDelete("deleteAccount")]
        public IActionResult DeleteAccount([FromQuery] string id)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(u => u.Id == id);

                if (user == null) {
                    return NotFound(new { message = "User not found." });
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                return Ok(new { message = "Account deleted successfully." });
            } catch (Exception ex) {
                return StatusCode(500, new { message = "An error occurred while deleting the account.", details = ex.Message });
            }
        }
                                
        public class LoginRequest {
            public required string Identifier { get; set; }
            public required string Password { get; set; }
        }

        public class CreateAccountRequest {
            public required string Name { get; set; }
            public required string Email { get; set; }
            public required string Password { get; set; }
            public required string ContactNumber { get; set; }
            public required string UserRole { get; set; }
            public required string Avatar { get; set; }
            public string? StudentID { get; set; } 
        }

        public class EditAccountRequest {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? ContactNumber { get; set; }
        }
    }
}