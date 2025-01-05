using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class SampleVisionController : ControllerBase {
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file) {

            if (file == null || file.Length == 0) {
                return BadRequest(new { error = "No file uploaded" });
            } else {
                try {
                    var recognitionResult = await CompVision.Recognise(file);
                    return Ok(recognitionResult);
                } catch (Exception ex) {
                    return StatusCode(500, new { error = ex.Message });
                }
            }
        }
    }
}