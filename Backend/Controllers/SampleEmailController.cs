using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleEmailController : ControllerBase {
        [HttpPost("send")]
        public async Task<IActionResult> SendTestEmail(string recipientEmail, string title, string template) {
            try {
                Emailer.CheckContext();
                var result = await Emailer.SendEmailAsync(recipientEmail, title, template);

                if (result.StartsWith("ERROR"))
                    return StatusCode(500, "Failed to send email. " + result.Substring("ERROR".Length));
                else if (result.StartsWith("UERROR"))
                    return StatusCode(500, "Failed to send email. " + result.Substring("UERROR".Length));
                else
                    return Ok("Email sent successfully.");
            } catch (Exception ex) {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}