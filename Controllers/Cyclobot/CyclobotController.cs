using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OpenAI;
using System;
using System.Collections.Generic;
using OpenAI.Chat;
using Backend.Services;

[Route("api/cyclobot")]
[ApiController]
public class CycloBotController : ControllerBase
{
    [HttpPost("queryCycloBotWithUserPrompt")]
    public async Task<IActionResult> QueryCycloBotWithUserPrompt(string userPrompt)
    {
        var api = new OpenAIChatService();
        var response = await api.PromptAsync(userPrompt);
        return Ok(response);
    }
}