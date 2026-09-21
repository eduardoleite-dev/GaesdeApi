using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using GaesdeApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IEnrollmentAccessService? _accessService;

    public QuizzesController(IQuizService quizService, IEnrollmentAccessService? accessService = null)
    {
        _quizService = quizService;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination)
    {
        var quizzes = await _quizService.GetAllAsync();
        if (IsStudent() && _accessService is not null)
        {
            var userId = GetUserId()!;
            var accessible = await Task.WhenAll(quizzes.Select(async quiz =>
                new { Quiz = quiz, Allowed = await _accessService.CanAccessQuizAsync(userId, quiz.Id, AccessLevel.Aluno) }));
            quizzes = accessible.Where(value => value.Allowed).Select(value => value.Quiz).ToArray();
        }
        return Ok(Utils.Paginate(quizzes, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var quiz = await _quizService.GetByIdAsync(id);
        if (quiz is not null && IsStudent() && _accessService is not null &&
            !await _accessService.CanAccessQuizAsync(GetUserId()!, id, AccessLevel.Aluno))
            return Forbid();
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

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsStudent() => User?.IsInRole(nameof(AccessLevel.Aluno)) == true;
}