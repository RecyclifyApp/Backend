// This controller is just to illustrate how to log messages using the Logger class.

using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleLogsController : ControllerBase {
        [HttpPost("log")]
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
    }
}