using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RewardItemController(MyDbContext _context) : ControllerBase
    {
        public class RewardItemRequest
        {
            public required string RewardTitle { get; set; }
            public required string RewardDescription { get; set; }
            public required int RequiredPoints { get; set; } = 0;
            public required int RewardQuantity { get; set; } = 0;
            public required bool IsAvailable { get; set; } = true;
            public string? ImageUrl { get; set; }
        }

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
        public async Task<IActionResult> CreateRewardItem([FromBody] RewardItemRequest rewardItemRequest)
        {
            // Ensure required fields are provided
            if (string.IsNullOrWhiteSpace(rewardItemRequest.RewardTitle) ||
                string.IsNullOrWhiteSpace(rewardItemRequest.RewardDescription) ||
                rewardItemRequest.RequiredPoints < 0 ||
                rewardItemRequest.RewardQuantity < 0)
            {
                return BadRequest(new { error = "UERROR: All fields are required and must be valid" });
            }

            // Generate a unique ID for the reward item
            var rewardItem = new RewardItem
            {
                RewardID = Utilities.GenerateUniqueID(), // Generate a random ID
                RewardTitle = rewardItemRequest.RewardTitle,
                RewardDescription = rewardItemRequest.RewardDescription,
                RequiredPoints = rewardItemRequest.RequiredPoints,
                RewardQuantity = rewardItemRequest.RewardQuantity,
                IsAvailable = rewardItemRequest.IsAvailable,
                ImageUrl = rewardItemRequest.ImageUrl
            };

            // Add the reward item to the context
            _context.RewardItems.Add(rewardItem);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return a 200 OK response with a success message
            return Ok(new { message = "SUCCESS: Reward item created successfully", data = rewardItem });
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