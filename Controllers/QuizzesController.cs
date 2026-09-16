using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination)
    {
        return Ok(Utils.Paginate(await _quizService.GetAllAsync(), pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var quiz = await _quizService.GetByIdAsync(id);
        return quiz is null ? NotFound() : Ok(quiz);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Create(CreateQuizRequestDto request)
    {
        var quiz = await _quizService.CreateAsync(request);
        return quiz is null
            ? Conflict(new { message = Messages.Quizzes.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Update(string id, UpdateQuizRequestDto request)
    {
        var quiz = await _quizService.UpdateAsync(id, request);
        return quiz is null
            ? NotFound(new { message = Messages.Quizzes.UpdateNotFound })
            : Ok(quiz);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _quizService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}