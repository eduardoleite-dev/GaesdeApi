using GaesdeApi.DTOs;
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
    public async Task<IActionResult> GetAll([FromQuery] string? quizId = null)
    {
        return Ok(await _questionService.GetAllAsync(quizId));
    }

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