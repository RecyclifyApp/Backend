using Backend.Services;
using Backend.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(CheckSystemLockedFilter))]
    public class ServicesController : ControllerBase {
        private readonly MyDbContext _context;

        public ServicesController(MyDbContext context) {
            _context = context;
        }

        [HttpPost("upload-file-to-firebase")]
        public async Task<IActionResult> UploadFile(IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest("Invalid file. Please upload a valid file.");
            }

            try {
                var result = await AssetsManager.UploadFileAsync(file);

                if (result.StartsWith("ERROR")) {
                    return StatusCode(500, result);
                }

                return Ok(new { message = "File uploaded successfully.", file.FileName });
            } catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("delete-file-from-firebase")]
        public async Task<IActionResult> DeleteFile(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return BadRequest("Invalid file name. Please provide a valid file name.");
            }

            try {
                var result = await AssetsManager.DeleteFileAsync(fileName);

                if (result.StartsWith("ERROR")) {
                    return StatusCode(500, result);
                }

                return Ok(new { message = "File deleted successfully.", fileName });
            } catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("get-file-url-from-firebase")]
        public async Task<IActionResult> GetFileUrl(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return BadRequest("Invalid file name. Please provide a valid file name.");
            }

            try {
                var result = await AssetsManager.GetFileUrlAsync(fileName);

                if (result.StartsWith("ERROR")) {
                    return StatusCode(500, result);
                }

                return Ok(result);
            } catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(string recipientEmail, string title, string template) {
            var emailVars = new Dictionary<string, string> {
                { "username", "Susie Jones" }
            };

            try {
                var Emailer = new Emailer(_context);
                var emailResult = await Emailer.SendEmailAsync(recipientEmail, title, template, emailVars);

                if (emailResult.StartsWith("ERROR"))
                    return StatusCode(500, "Failed to send email. " + emailResult.Substring("ERROR".Length));
                else if (emailResult.StartsWith("UERROR"))
                    return StatusCode(500, "Failed to send email. " + emailResult.Substring("UERROR".Length));
                else
                    return Ok("Email dispatched successfully.");
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("log-message")]
        public IActionResult LogMessage(string message) {
            if (string.IsNullOrEmpty(message)) {
                return BadRequest("Invalid message. Please provide a valid message.");
            }

            try {
                Logger.Log(message);
                return Ok(new { message = "Message logged successfully." });
            } catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("send-sms")]
        public async Task<IActionResult> SendSMS(string recipientNo, string message) {
            try {
                var SmsService = new SmsService(_context);
                var smsResult = await SmsService.SendSmsAsync(recipientNo, message);

                if (smsResult.StartsWith("ERROR"))
                    return StatusCode(500, "Failed to send SMS. " + smsResult.Substring("ERROR".Length));
                else if (smsResult.StartsWith("UERROR"))
                    return StatusCode(500, "Failed to send SMS. " + smsResult.Substring("UERROR".Length));
                else
                    return Ok("SMS dispatched successfully.");
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("generate-uuid")]
        public IActionResult GenerateUUID(int customLength = 0) {
            return Ok(new { uuid = Utilities.GenerateUniqueID(customLength) });
        }

        [HttpGet("generate-random-int")]
        public IActionResult GenerateRandomInt(int min, int max) {
            return Ok(new { randomInt = Utilities.GenerateRandomInt(min, max) });
        }

        [HttpGet("hash-string")]
        public IActionResult HashString(string input) {
            if (string.IsNullOrEmpty(input)) {
                return BadRequest("Invalid input. Please provide a valid string to hash.");
            }

            return Ok(new { hash = Utilities.HashString(input) });
        }

        [HttpGet("encode-base64")]
        public IActionResult EncodeToBase64(string input) {
            if (string.IsNullOrEmpty(input)) {
                return BadRequest("Invalid input. Please provide a valid string to encode.");
            }

            return Ok(new { encoded = Utilities.EncodeToBase64(input) });
        }

        [HttpGet("decode-base64")]
        public IActionResult DecodeFromBase64(string input) {
            if (string.IsNullOrEmpty(input)) {
                return BadRequest("Invalid input. Please provide a valid string to decode.");
            }

            return Ok(new { decoded = Utilities.DecodeFromBase64(input) });
        }

        [HttpPost("image-recognition")]
        public async Task<IActionResult> UploadImage(IFormFile file) {
            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "No file uploaded" });
            } else {
                try {
                    var compVision = new CompVision(_context);
                    var recognitionResult = await compVision.Recognise(file);
                    return Ok(recognitionResult);
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }

        [HttpGet("reccomend-quests")]
        public async Task<IActionResult> GetRecommendedQuests(string classID) {
            if (string.IsNullOrEmpty(classID)) {
                return BadRequest(new { error = "Invalid Class ID. Please provide a valid Class ID." });
            }

            if (!await _context.Classes.AnyAsync(c => c.ClassID == classID)) {
                return NotFound(new { error = "Class not found" });
            }

            try {
                var recommendQuests = await ReccommendationsManager.RecommendQuestsAsync(_context, classID, 3);
                if (recommendQuests == null) {
                    return NotFound(new { error = "Class has not completed any quests yet" });
                }

                return Ok(recommendQuests);
            } catch (Exception ex) {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}