using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;
using Backend.Filters;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class RewardItemController(MyDbContext _context) : ControllerBase {
        public class RewardItemRequest {
            public required string RewardTitle { get; set; }
            public required string RewardDescription { get; set; }
            public int RequiredPoints { get; set; }
            public int RewardQuantity { get; set; }
            public bool IsAvailable { get; set; }
            public required IFormFile ImageFile { get; set; }
        }


        [HttpGet]
        public async Task<IActionResult> GetRewardItems() {
            var rewardItems = await _context.RewardItems.ToListAsync();
            return Ok(new { message = "SUCCESS: Reward items retrieved", data = rewardItems });
        }

        [HttpGet("{rewardID}")]
        public async Task<IActionResult> GetRewardItem(string rewardID) {
            var rewardItem = await _context.RewardItems
                .Where(item => item.RewardID == rewardID)
                .FirstOrDefaultAsync(); // Get a single matching reward item

            if (rewardItem == null) {
                return NotFound(new { error = "ERROR: Reward item not found" });
            }

            return Ok(new { message = "SUCCESS: Reward item retrieved", data = rewardItem });
        }


        [HttpPost]
        public async Task<IActionResult> CreateRewardItem([FromForm] RewardItemRequest rewardItemRequest) {

            if (rewardItemRequest.ImageFile == null) {
                return BadRequest(new { error = "Image file is missing!" });
            }

            // Ensure required fields are provided
            if (string.IsNullOrWhiteSpace(rewardItemRequest.RewardTitle) || string.IsNullOrWhiteSpace(rewardItemRequest.RewardDescription) || rewardItemRequest.RequiredPoints < 0 || rewardItemRequest.RewardQuantity < 0) {
                return BadRequest(new { error = "UERROR: All fields are required and must be valid" });
            }

            string? imageUrl = null;

            // Handle image upload if a file is provided
            if (rewardItemRequest.ImageFile != null && rewardItemRequest.ImageFile.Length > 0) {
                try {
                    // Generate a unique filename
                    string newFileName = $"{Guid.NewGuid()}_{rewardItemRequest.ImageFile.FileName}";

                    using (var memoryStream = new MemoryStream()) {
                        await rewardItemRequest.ImageFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // Reset stream position

                        // Create new IFormFile with the modified filename
                        var renamedFile = new FormFile(memoryStream, 0, rewardItemRequest.ImageFile.Length, rewardItemRequest.ImageFile.Name, newFileName) {
                            Headers = rewardItemRequest.ImageFile.Headers,
                            ContentType = rewardItemRequest.ImageFile.ContentType
                        };

                        // Upload the file (Using AssetsManager like in EditAvatar method)
                        var uploadResult = await AssetsManager.UploadFileAsync(renamedFile);
                        if (uploadResult.StartsWith("ERROR")) {
                            return StatusCode(500, new { error = "ERROR: Failed to upload image." });
                        }

                        // Get image URL
                        var getImageUrlResult = await AssetsManager.GetFileUrlAsync(newFileName);
                        imageUrl = getImageUrlResult.StartsWith("SUCCESS: ") ? getImageUrlResult.Substring(9) : getImageUrlResult;
                    }
                } catch (Exception ex) {
                    return StatusCode(500, new { error = "ERROR: Image upload failed.", details = ex.Message });
                }
            }

            // Create the reward item
            var rewardItem = new RewardItem {
                RewardID = Guid.NewGuid().ToString(), // Generate unique ID
                RewardTitle = rewardItemRequest.RewardTitle,
                RewardDescription = rewardItemRequest.RewardDescription,
                RequiredPoints = rewardItemRequest.RequiredPoints,
                RewardQuantity = rewardItemRequest.RewardQuantity,
                IsAvailable = rewardItemRequest.IsAvailable,
                ImageUrl = imageUrl // Store uploaded image URL
            };

            // Save to database
            _context.RewardItems.Add(rewardItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "SUCCESS: Reward item created successfully" });
        }

        [HttpPut("{rewardID}")]
        public async Task<IActionResult> UpdateRewardItem(string rewardID, [FromBody] RewardItem updatedItem) {
            if (rewardID != updatedItem.RewardID) {
                return BadRequest(new { error = "UERROR: Reward ID mismatch" });
            }

            _context.Entry(updatedItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "SUCCESS: Reward item updated", data = updatedItem });
        }

        [HttpPut("{rewardID}/toggle-availability")]
        public async Task<IActionResult> ToggleAvailability(string rewardID) {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);
            if (rewardItem == null) {
                return NotFound(new { error = "ERROR: Reward item not found" });
            }

            rewardItem.IsAvailable = !rewardItem.IsAvailable;
            _context.Entry(rewardItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "SUCCESS: Availability toggled successfully", data = rewardItem });
        }

        [HttpGet("{rewardID}/getImageUrl")]
        public async Task<IActionResult> GetImageUrl(string rewardID) {
            var rewardItem = await _context.RewardItems.FindAsync(rewardID);

            if (rewardItem == null) {
                return NotFound(new { message = "ERROR: Reward item not found", data = (string?)null });
            }

            return Ok(new { message = "SUCCESS: Image URL retrieved", data = rewardItem.ImageUrl });
        }
    }
}