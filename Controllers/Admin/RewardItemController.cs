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
            var rewardItems = await _context.RewardItems.ToListAsync();
            return Ok(new { message = "SUCCESS: Reward items retrieved", data = rewardItems });
        }

        [HttpGet("{rewardID}")]
        public async Task<IActionResult> GetRewardItem(string rewardID)
        {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null || !rewardItem.IsAvailable)
            {
                return NotFound(new { error = "ERROR: Reward item not found" });
            }

            return Ok(new { message = "SUCCESS: Reward item retrieved", data = rewardItem });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRewardItem([FromBody] RewardItem rewardItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "UERROR: Invalid reward item data" });
            }

            _context.RewardItems.Add(rewardItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetRewardItem), 
                new { rewardID = rewardItem.RewardID }, 
                new { message = "SUCCESS: Reward item created", data = rewardItem }
            );
        }

        [HttpPut("{rewardID}")]
        public async Task<IActionResult> UpdateRewardItem(string rewardID, [FromBody] RewardItem updatedItem)
        {
            if (rewardID != updatedItem.RewardID)
            {
                return BadRequest(new { error = "UERROR: Reward ID mismatch" });
            }

            _context.Entry(updatedItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "SUCCESS: Reward item updated", data = updatedItem });
        }

        [HttpPut("{rewardID}/toggle-availability")]
        public async Task<IActionResult> ToggleAvailability(string rewardID)
        {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null)
            {
                return NotFound(new { error = "ERROR: Reward item not found" });
            }

            rewardItem.IsAvailable = !rewardItem.IsAvailable;
            _context.Entry(rewardItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "SUCCESS: Availability toggled successfully", data = rewardItem });
        }
    }
}