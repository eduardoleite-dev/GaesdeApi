using GaesdeApi.DTOs;
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
        var answers = await _userAnswerService.GetAllAsync(attemptId);
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
        var answer = await _userAnswerService.GetByIdAsync(id);
        return answer is null ? NotFound() : Ok(answer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserAnswerRequestDto request)
    {
        var answer = await _userAnswerService.CreateAsync(request);
        return answer is null
            ? Conflict(new { message = Messages.UserAnswers.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = answer.Id }, answer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserAnswerRequestDto request)
    {
        var answer = await _userAnswerService.UpdateAsync(id, request);
        return answer is null
            ? NotFound(new { message = Messages.UserAnswers.UpdateNotFound })
            : Ok(answer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _userAnswerService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}