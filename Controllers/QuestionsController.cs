using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly IEnrollmentAccessService? _accessService;
    private readonly ICloudinaryService? _cloudinaryService;

    public QuestionsController(IQuestionService questionService, ICloudinaryService? cloudinaryService = null, IEnrollmentAccessService? accessService = null)
    {
        _questionService = questionService;
        _cloudinaryService = cloudinaryService;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? quizId = null, [FromQuery] QuestionType? type = null)
    {
        var questions = await _questionService.GetAllAsync(quizId);
        if (Utils.IsStudent(User))
        {
            if (string.IsNullOrWhiteSpace(quizId) || _accessService is null ||
                !await _accessService.CanAccessQuizAsync(Utils.GetUserId(User)!, quizId, AccessLevel.Aluno))
                return Forbid();
        }
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
        if (question is not null && Utils.IsStudent(User) && _accessService is not null &&
            !await _accessService.CanAccessQuestionAsync(Utils.GetUserId(User)!, id, AccessLevel.Aluno))
            return Forbid();
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

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(string id)
    {
        var question = await _questionService.GetByIdAsync(id);
        if (question is not null && Utils.IsStudent(User) && _accessService is not null &&
            !await _accessService.CanAccessQuestionAsync(Utils.GetUserId(User)!, id, AccessLevel.Aluno))
            return Forbid();
        return question is null || string.IsNullOrWhiteSpace(question.PhotoUrl)
            ? NotFound()
            : Ok(new { url = question.PhotoUrl });
    }

    [HttpPost("{id}/photo")]
    [Authorize(Roles = "Professor,Administrador")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> UploadPhoto(string id, [FromForm] UploadMediaRequestDto request) => SavePhoto(id, request);

    [HttpPut("{id}/photo")]
    [Authorize(Roles = "Professor,Administrador")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> UpdatePhoto(string id, [FromForm] UploadMediaRequestDto request) => SavePhoto(id, request);

    private async Task<IActionResult> SavePhoto(string id, UploadMediaRequestDto request)
    {
        var question = await _questionService.GetByIdAsync(id);
        if (question is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("questao", question.QuestionText, question.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/questions");
            var updatedQuestion = await _questionService.UpdatePhotoAsync(id, result.Url);
            return updatedQuestion is null ? NotFound() : Ok(updatedQuestion);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = exception.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _questionService.DeleteAsync(id) ? NoContent() : NotFound();
    }

    private string? GetUserId() => Utils.GetUserId(User);
}