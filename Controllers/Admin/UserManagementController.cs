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
    }
}