using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Filters;
using Backend;

[ApiController]
[Route("api/chat-completion")]
[ServiceFilter(typeof(CheckSystemLockedFilter))]
public class EcoPilotController(MyDbContext context) : ControllerBase {
    private readonly MyDbContext _context = context;

    [HttpPost("prompt")]
    public async Task<IActionResult> QueryCycloBotWithUserPrompt([FromBody] UserPromptRequest request) {
        if (string.IsNullOrEmpty(request.UserPrompt)) {
           return BadRequest(new { error = "UERROR: User Prompt is required" });
        }

        var api = new OpenAIChatService(_context);
        var response = await api.PromptAsync(request.UserPrompt);
        return Ok(response);
    }
}

public class UserPromptRequest {
    public required string UserPrompt { get; set; }
}