using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController(MyDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(new { message = "SUCCESS: Users retrieved", data = users });
        }

        [HttpPut("{id}")]
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
    }
}