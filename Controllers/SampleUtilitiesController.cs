// This controller is just to illustrate how to use the various Utility Tools using the Utilities class.

using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleUtilitiesController : ControllerBase {
        [HttpGet("generate-uuid")]
        public IActionResult GenerateUUID(int customLength = 0) {
            return Ok(new { uuid = Utilities.GenerateUniqueID(customLength) });
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
    }
}