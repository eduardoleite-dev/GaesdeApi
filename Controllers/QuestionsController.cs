using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? quizId = null, [FromQuery] QuestionType? type = null)
    {
        var questions = await _questionService.GetAllAsync(quizId);
        if (type.HasValue)
            questions = questions.Where(question => question.Type == type.Value).ToArray();
        return Ok(Utils.Paginate(questions, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll(string? quizId) => GetAll(new PaginationRequest(), quizId);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var question = await _questionService.GetByIdAsync(id);
        return question is null ? NotFound() : Ok(question);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Create(CreateQuestionRequestDto request)
    {
        var question = await _questionService.CreateAsync(request);
        return question is null
            ? Conflict(new { message = Messages.Questions.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Update(string id, UpdateQuestionRequestDto request)
    {
        var question = await _questionService.UpdateAsync(id, request);
        return question is null
            ? NotFound(new { message = Messages.Questions.UpdateNotFound })
            : Ok(question);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _questionService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}