using GaesdeApi.DTOs;
using System.Security.Claims;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserAnswersController : ControllerBase
{
    private readonly IUserAnswerService _userAnswerService;

    public UserAnswersController(IUserAnswerService userAnswerService)
    {
        _userAnswerService = userAnswerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? attemptId = null, [FromQuery] string? questionId = null, [FromQuery] bool? isCorrect = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var answers = await _userAnswerService.GetAllAsync(attemptId, userId, IsAdministrator());
        if (!string.IsNullOrWhiteSpace(questionId))
            answers = answers.Where(answer => answer.QuestionId == questionId).ToArray();
        if (isCorrect.HasValue)
            answers = answers.Where(answer => answer.IsCorrect == isCorrect.Value).ToArray();
        return Ok(Utils.Paginate(answers, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll(string? attemptId) => GetAll(new PaginationRequest(), attemptId);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var answer = await _userAnswerService.GetByIdAsync(id, userId, IsAdministrator());
        return answer is null ? NotFound() : Ok(answer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserAnswerRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var answer = await _userAnswerService.CreateAsync(request, userId);
        return answer is null
            ? Conflict(new { message = Messages.UserAnswers.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = answer.Id }, answer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserAnswerRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var answer = await _userAnswerService.UpdateAsync(id, request, userId, IsAdministrator());
        return answer is null
            ? NotFound(new { message = Messages.UserAnswers.UpdateNotFound })
            : Ok(answer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        return await _userAnswerService.DeleteAsync(id, userId, IsAdministrator()) ? NoContent() : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsAdministrator() => User.IsInRole("Administrador");
}