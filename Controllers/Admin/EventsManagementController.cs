using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    public class EventRequest
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string EventDateTime { get; set; }
        public required IFormFile ImageFile { get; set; } // For image file upload
    }

    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(MyDbContext _context) : ControllerBase
    {

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                // Retrieve all events from the database
                var events = await _context.Events.ToListAsync();

                // Return the events in a response
                return Ok(new { events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "ERROR: Failed to retrieve events.", details = ex.Message });
            }
        }

        // POST: api/Events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromForm] EventRequest eventRequest)
        {
            // Validate the input fields
            if (string.IsNullOrWhiteSpace(eventRequest.Title) ||
                string.IsNullOrWhiteSpace(eventRequest.Description) ||
                string.IsNullOrWhiteSpace(eventRequest.EventDateTime))
            {
                return BadRequest(new { error = "ERROR: All fields are required and must be valid" });
            }

            // Initialize the image URL (if any image is uploaded)
            string? imageUrl = null;

            // Handle image file upload if provided
            if (eventRequest.ImageFile != null && eventRequest.ImageFile.Length > 0)
            {
                try
                {
                    // Generate a unique filename for the image
                    string newFileName = $"{Guid.NewGuid()}_{eventRequest.ImageFile.FileName}";

                    using (var memoryStream = new MemoryStream())
                    {
                        // Copy the file content into memory stream
                        await eventRequest.ImageFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // Reset stream position

                        // Create a new IFormFile with the modified filename
                        var renamedFile = new FormFile(memoryStream, 0, eventRequest.ImageFile.Length, eventRequest.ImageFile.Name, newFileName)
                        {
                            Headers = eventRequest.ImageFile.Headers,
                            ContentType = eventRequest.ImageFile.ContentType
                        };

                        // Upload the image file (assuming you have an AssetManager or similar utility for file upload)
                        var uploadResult = await AssetsManager.UploadFileAsync(renamedFile);
                        if (uploadResult.StartsWith("ERROR"))
                        {
                            return StatusCode(500, new { error = "ERROR: Failed to upload image." });
                        }

                        // Get the file URL after the upload is successful
                        var getImageUrlResult = await AssetsManager.GetFileUrlAsync(newFileName);
                        imageUrl = getImageUrlResult.StartsWith("SUCCESS: ") ? getImageUrlResult.Substring(9) : getImageUrlResult;
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { error = "ERROR: Image upload failed.", details = ex.Message });
                }
            }

            // Create a new Event object
            var newEvent = new Event
            {
                Id = new Random().Next(100, 1000),
                Title = eventRequest.Title,
                Description = eventRequest.Description,
                EventDateTime = eventRequest.EventDateTime,
                PostedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ImageUrl = imageUrl // Store the image URL if uploaded
            };

            // Save the event to the database
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "SUCCESS: Event created successfully" });
        }
    }
}
