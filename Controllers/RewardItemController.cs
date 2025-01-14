using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardItemController : ControllerBase
    {
        private readonly MyDbContext _context;

        public RewardItemController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/RewardItems
        [HttpGet]
        public async Task<IActionResult> GetRewardItems()
        {
            var rewardItems = await _context.RewardItems.ToListAsync();
            return Ok(rewardItems);
        }

        // GET: api/RewardItems/{rewardID}
        [HttpGet("{rewardID}")]
        public async Task<IActionResult> GetRewardItem(string rewardID)
        {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null)
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

        // DELETE: api/RewardItems/{rewardID}
        [HttpDelete("{rewardID}")]
        public async Task<IActionResult> DeleteRewardItem(string rewardID)
        {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null)
            {
                return NotFound();
            }

            _context.RewardItems.Remove(rewardItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}