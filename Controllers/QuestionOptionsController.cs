using GaesdeApi.DTOs;
using GaesdeApi.Models;
using System.Security.Claims;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionOptionsController : ControllerBase
{
    private readonly IQuestionOptionService _questionOptionService;
    private readonly IEnrollmentAccessService? _accessService;

    public QuestionOptionsController(IQuestionOptionService questionOptionService, IEnrollmentAccessService? accessService = null)
    {
        _questionOptionService = questionOptionService;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? questionId = null, [FromQuery] bool? isCorrect = null)
    {
        var options = await _questionOptionService.GetAllAsync(questionId);
        var isStudent = Utils.IsStudent(User);
        if (isStudent)
        {
            if (string.IsNullOrWhiteSpace(questionId) || _accessService is null ||
                !await _accessService.CanAccessQuestionAsync(Utils.GetUserId(User)!, questionId, AccessLevel.Aluno))
                return Forbid();
        }
        if (isCorrect.HasValue && !isStudent)
            options = options.Where(option => option.IsCorrect == isCorrect.Value).ToArray();
        if (isStudent)
            return Ok(Utils.Paginate(options.Select(ToStudentResponse), pagination));
        return Ok(Utils.Paginate(options, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll(string? questionId) => GetAll(new PaginationRequest(), questionId);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var option = await _questionOptionService.GetByIdAsync(id);
        var isStudent = Utils.IsStudent(User);
        if (option is not null && isStudent && _accessService is not null &&
            !await _accessService.CanAccessOptionAsync(Utils.GetUserId(User)!, id, AccessLevel.Aluno))
            return Forbid();
        return option is null
            ? NotFound()
            : isStudent
                ? Ok(ToStudentResponse(option))
                : Ok(option);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Create(CreateQuestionOptionRequestDto request)
    {
        var option = await _questionOptionService.CreateAsync(request);
        return option is null
            ? Conflict(new { message = Messages.QuestionOptions.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = option.Id }, option);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Update(string id, UpdateQuestionOptionRequestDto request)
    {
        var option = await _questionOptionService.UpdateAsync(id, request);
        return option is null
            ? NotFound(new { message = Messages.QuestionOptions.UpdateNotFound })
            : Ok(option);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _questionOptionService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    private string? GetUserId() => Utils.GetUserId(User);

    private static StudentQuestionOptionResponseDto ToStudentResponse(QuestionOptionResponseDto option) => new(
        option.Id,
        option.QuestionId,
        option.OptionText,
        option.CreatedAt);
}