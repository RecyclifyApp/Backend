// This controller is just to illustrate how to dispatch emails using the Emailer class.

using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleEmailController : ControllerBase {
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(string recipientEmail, string title, string template) {
            var emailVars = new Dictionary<string, string> {
                { "username", "Susie Jones" }
            };

            try {
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
    }
}