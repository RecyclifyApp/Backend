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
            // Fetch all items from the database, including those with isAvailable = false
            var rewardItems = await _context.Users.ToListAsync();
            return Ok(rewardItems);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            // Update only allowed fields
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.ContactNumber = updatedUser.ContactNumber;
            existingUser.UserRole = updatedUser.UserRole;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(existingUser);
            }
            catch
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }
    }
}