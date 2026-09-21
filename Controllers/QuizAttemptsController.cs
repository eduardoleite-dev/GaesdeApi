using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/quiz-attempts")]
[Authorize]
public class QuizAttemptsController : ControllerBase
{
    private readonly IQuizAttemptService _attemptService;

    public QuizAttemptsController(IQuizAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? quizId = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var attempts = await _attemptService.GetAllAsync(userId, IsAdministrator(), quizId);
        return Ok(Utils.Paginate(attempts, pagination));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var attempt = await _attemptService.GetByIdAsync(id, userId, IsAdministrator());
        return attempt is null ? NotFound() : Ok(attempt);
    }

    [HttpPost]
    public async Task<IActionResult> Start(StartQuizAttemptRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var attempt = await _attemptService.StartAsync(userId, request);
        return attempt is null
            ? Conflict(new { message = "Quiz inexistente, matrícula inválida ou limite de tentativas atingido." })
            : CreatedAtAction(nameof(GetById), new { id = attempt.Id }, attempt);
    }

    [HttpPatch("{id}/finish")]
    public async Task<IActionResult> Finish(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var attempt = await _attemptService.FinishAsync(id, userId, IsAdministrator());
        return attempt is null ? BadRequest() : Ok(attempt);
    }

    [HttpPatch("{id}/abandon")]
    public async Task<IActionResult> Abandon(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var attempt = await _attemptService.AbandonAsync(id, userId, IsAdministrator());
        return attempt is null ? BadRequest() : Ok(attempt);
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsAdministrator() => User.IsInRole("Administrador");
}
