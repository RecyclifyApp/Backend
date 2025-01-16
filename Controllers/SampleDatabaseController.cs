// This controller is just to illustrate how to manage basic DB operations using the SampleDatabaseController class.
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleDatabaseController (MyDbContext context) : ControllerBase {
        private readonly MyDbContext _context = context;
        
        [HttpPost("populate-db")]
        public async Task<IActionResult> PopulateDatabase() {
            await DatabaseManager.CleanAndPopulateDatabase(_context);
            return Ok(new { message = "Database populated successfully" });
        }
    }
}
