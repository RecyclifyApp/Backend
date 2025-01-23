using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardItemController(MyDbContext _context) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetRewardItems()
        {
            // Fetch all items from the database, including those with isAvailable = false
            var rewardItems = await _context.RewardItems.ToListAsync();
            return Ok(rewardItems);
        }

        // GET: api/RewardItems/{rewardID}
        [HttpGet("{rewardID}")]
        public async Task<IActionResult> GetRewardItem(string rewardID)
        {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null || !rewardItem.IsAvailable)
            {
                return NotFound();
            }

            return Ok(rewardItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRewardItem([FromBody] RewardItem rewardItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.RewardItems.Add(rewardItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRewardItem), new { rewardID = rewardItem.RewardID }, rewardItem);
        }

        // PUT: api/RewardItems/{rewardID}
        [HttpPut("{rewardID}")]
        public async Task<IActionResult> UpdateRewardItem(string rewardID, [FromBody] RewardItem updatedItem)
        {
            if (rewardID != updatedItem.RewardID)
            {
                return BadRequest();
            }

            _context.Entry(updatedItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/RewardItems/{rewardID}/toggle-availability
        [HttpPut("{rewardID}/toggle-availability")]
        public async Task<IActionResult> ToggleAvailability(string rewardID)
        {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null)
            {
                return NotFound();
            }

            // Toggle the IsAvailable status
            rewardItem.IsAvailable = !rewardItem.IsAvailable;
            _context.Entry(rewardItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok( new { message = "SUCCESS: Availability toggled successfully.", data = rewardItem });
        }
    }
}