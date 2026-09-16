using GaesdeApi.DTOs;
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

    public QuestionOptionsController(IQuestionOptionService questionOptionService)
    {
        _questionOptionService = questionOptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? questionId = null)
    {
        return Ok(await _questionOptionService.GetAllAsync(questionId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var option = await _questionOptionService.GetByIdAsync(id);
        return option is null ? NotFound() : Ok(option);
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
}