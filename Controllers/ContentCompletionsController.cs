using System.Security.Claims;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/content-completions")]
[Authorize(Roles = "Aluno")]
public class ContentCompletionsController : ControllerBase
{
    private readonly IContentCompletionService _completionService;

    public ContentCompletionsController(IContentCompletionService completionService)
    {
        _completionService = completionService;
    }

    [HttpPost("{contentId}")]
    public async Task<IActionResult> Complete(string contentId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var completion = await _completionService.CompleteAsync(userId, contentId);
        return completion is null
            ? BadRequest(new { message = "Conteúdo inexistente, matrícula inativa ou conteúdo anterior pendente." })
            : Ok(completion);
    }

    [HttpDelete("{contentId}")]
    public async Task<IActionResult> Uncomplete(string contentId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        return await _completionService.UncompleteAsync(userId, contentId)
            ? NoContent()
            : NotFound();
    }

    [HttpGet("course/{courseId}/progress")]
    public async Task<IActionResult> GetProgress(string courseId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var progress = await _completionService.GetProgressAsync(userId, courseId);
        return progress is null ? NotFound() : Ok(progress);
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
