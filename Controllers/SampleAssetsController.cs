// This controller is just to illustrate how to manage file upload using the AssetsManager class.
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SampleAssetsController : ControllerBase {

        [HttpPost("upload-file")]
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

        [HttpDelete("delete-file")]
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

        [HttpGet("get-file-url")]
        public async Task<IActionResult> RetrieveFileUrl(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                return BadRequest("Invalid file name. Please provide a valid file name.");
            }

            try {
                var result = await AssetsManager.GetFileUrlAsync(fileName);

                if (result.StartsWith("ERROR")) {
                    return StatusCode(500, result);
                }

                return Ok(new { message = "File URL retrieved successfully.", url = result });
            } catch (Exception ex) {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
