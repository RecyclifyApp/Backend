using Microsoft.AspNetCore.Mvc;
using Backend.Services;

[Route("api/chat-completion")]
[ApiController]
public class EcoPilotController : ControllerBase {
    [HttpPost("prompt")]
    public async Task<IActionResult> QueryCycloBotWithUserPrompt(string userPrompt) {
        var api = new OpenAIChatService();
        var response = await api.PromptAsync(userPrompt);
        return Ok(response);
    }
}