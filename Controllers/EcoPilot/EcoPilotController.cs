using Microsoft.AspNetCore.Mvc;
using Backend.Services;

[Route("api/chat-completion")]
[ApiController]
public class EcoPilotController : ControllerBase
{
    [HttpPost("prompt")]
    public async Task<IActionResult> QueryCycloBotWithUserPrompt([FromBody] UserPromptRequest request)
    {
        if (string.IsNullOrEmpty(request.UserPrompt))
        {
           return BadRequest(new { error = "UERROR: User Prompt is required" });
        }

        var api = new OpenAIChatService();
        var response = await api.PromptAsync(request.UserPrompt);
        return Ok(response);
    }
}

public class UserPromptRequest
{
    public required string UserPrompt { get; set; }
}