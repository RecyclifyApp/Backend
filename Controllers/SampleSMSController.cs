// This controller is just to illustrate how to dispatch SMS texts using the SmsService class.

using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleSMSController : ControllerBase {
        [HttpPost("send-sms")]
        public async Task<IActionResult> SendSMS(string recipientNo, string message) {
            SmsService.CheckContext();
            
            try {
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
    }
}