using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers.Identity
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly MyDbContext _context;

        public IdentityController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public IActionResult CreateAccount([FromBody] CreateAccountRequest request)
        {
            var keyValuePairs = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Id", request.Id },
                    { "Name", request.Name },
                    { "Email", request.Email },
                    { "Password", request.Password },
                    { "ContactNumber", request.ContactNumber },
                    { "UserRole", request.UserRole },
                    { "Avatar", request.Avatar }
                }
            };

            if (request.UserRole == "parent" && !string.IsNullOrEmpty(request.StudentID))
            {
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
    }

    public class CreateAccountRequest
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ContactNumber { get; set; }
        public required string UserRole { get; set; }
        public required string Avatar { get; set; }
        public string? StudentID { get; set; } // Optional for parent role
    }
}