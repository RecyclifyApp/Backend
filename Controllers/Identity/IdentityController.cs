using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using FirebaseAdmin.Messaging;

namespace Backend.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public IdentityController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.SingleOrDefault(u =>
                (u.Email == request.Identifier || u.Name == request.Identifier) &&
                u.Password == Utilities.HashString(request.Password));

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid login credentials." });
            }

            // Return a simple success response (JWT or other tokens can be added here)
            // return Ok(new { message = "Login successful.", user = new { user.Id, user.Name, user.Email, user.UserRole } });
            return Ok(new {message = "Login Successful"});
        }

        public class LoginRequest
        {
            public required string Identifier { get; set; } // Username or Email
            public required string Password { get; set; }
        }

        [HttpPost("create")]
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

            if (request.UserRole == "parent")
            {
                if (string.IsNullOrEmpty(request.StudentID))
                {
                    return BadRequest(new { Error = "StudentID cannot be empty for a user with the 'parent' role." });
                }
                keyValuePairs[0].Add("StudentID", request.StudentID);
            }

            try
            {
                DatabaseManager.CreateUserRecords(_context, request.UserRole, keyValuePairs);
                return Ok("Account created successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the account.", details = ex.Message });
            }
        }

        public class CreateAccountRequest
        {
            public required string Name { get; set; }
            public required string Email { get; set; }
            public required string Password { get; set; }
            public required string ContactNumber { get; set; }
            public required string UserRole { get; set; }
            public required string Avatar { get; set; }
            public string? StudentID { get; set; } 
        }

        [HttpDelete("delete")]
        public IActionResult DeleteAccount([FromQuery] string id)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                return Ok(new { message = "Account deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the account.", details = ex.Message });
            }
        }
    }
}