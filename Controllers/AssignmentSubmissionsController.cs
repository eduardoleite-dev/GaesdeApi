using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentSubmissionsController : ControllerBase
{
    private readonly IAssignmentSubmissionService _submissionService;

    public AssignmentSubmissionsController(IAssignmentSubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? contentId = null, [FromQuery] string? enrollmentId = null, [FromQuery] bool? graded = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var submissions = await _submissionService.GetAllAsync(userId, User.IsInRole("Administrador"));
        if (!string.IsNullOrWhiteSpace(contentId))
            submissions = submissions.Where(submission => submission.ContentId == contentId).ToArray();
        if (!string.IsNullOrWhiteSpace(enrollmentId))
            submissions = submissions.Where(submission => submission.EnrollmentId == enrollmentId).ToArray();
        if (graded.HasValue)
            submissions = submissions.Where(submission => submission.IsGraded == graded.Value).ToArray();
        return Ok(Utils.Paginate(submissions, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var submission = await _submissionService.GetByIdAsync(id, userId, User.IsInRole("Administrador"));
        return submission is null ? NotFound() : Ok(submission);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAssignmentSubmissionRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var submission = await _submissionService.CreateAsync(userId, request);
        return submission is null
            ? Conflict(new { message = Messages.AssignmentSubmissions.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = submission.Id }, submission);
    }

    [HttpPatch("{id}/grade")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Grade(string id, GradeAssignmentSubmissionRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var submission = await _submissionService.GradeAsync(id, userId, User.IsInRole("Administrador"), request);
        return submission is null
            ? NotFound(new { message = Messages.AssignmentSubmissions.GradeNotFound })
            : Ok(submission);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        return await _submissionService.DeleteAsync(id, userId, User.IsInRole("Administrador"))
            ? NoContent()
            : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
}