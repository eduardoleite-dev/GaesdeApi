using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICloudinaryService? _cloudinaryService;

    public EnrollmentsController(IEnrollmentService enrollmentService, ICloudinaryService? cloudinaryService = null)
    {
        _enrollmentService = enrollmentService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] EnrollmentStatus? status = null, [FromQuery] string? courseId = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var enrollments = await _enrollmentService.GetAllAsync(userId, IsAdministrator());
        if (status.HasValue)
            enrollments = enrollments.Where(enrollment => enrollment.Status == status.Value).ToArray();
        if (!string.IsNullOrWhiteSpace(courseId))
            enrollments = enrollments.Where(enrollment => enrollment.CourseId == courseId).ToArray();
        return Ok(Utils.Paginate(enrollments, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("me")]
    public async Task<IActionResult> GetMine([FromQuery] PaginationRequest pagination, [FromQuery] EnrollmentStatus? status = null, [FromQuery] string? courseId = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var enrollments = await _enrollmentService.GetAllAsync(userId, false);
        if (status.HasValue)
            enrollments = enrollments.Where(enrollment => enrollment.Status == status.Value).ToArray();
        if (!string.IsNullOrWhiteSpace(courseId))
            enrollments = enrollments.Where(enrollment => enrollment.CourseId == courseId).ToArray();
        return Ok(Utils.Paginate(enrollments, pagination));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var enrollment = await _enrollmentService.GetByIdAsync(id, userId, IsAdministrator());
        return enrollment is null ? NotFound() : Ok(enrollment);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador,Vendedor")]
    public async Task<IActionResult> Create(CreateEnrollmentRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var canEnrollOtherUsers = User.IsInRole(nameof(AccessLevel.Administrador)) ||
            User.IsInRole(nameof(AccessLevel.Professor)) ||
            User.IsInRole(nameof(AccessLevel.Vendedor));
        var enrollment = await _enrollmentService.CreateAsync(userId, canEnrollOtherUsers, request);
        return enrollment is null
            ? Conflict(new { message = Messages.Enrollments.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(string id, UpdateEnrollmentProgressRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var enrollment = await _enrollmentService.UpdateProgressAsync(
            id, userId, IsAdministrator(), request.ProgressPercentage);
        return enrollment is null
            ? BadRequest(new { message = Messages.Enrollments.InvalidProgressOrNotFound })
            : Ok(enrollment);
    }

    [HttpPatch("{id}/status/{status}")]
    public async Task<IActionResult> UpdateStatus(string id, EnrollmentStatus status)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var enrollment = await _enrollmentService.UpdateStatusAsync(id, userId, IsAdministrator(), status);
        return enrollment is null ? NotFound() : Ok(enrollment);
    }

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var enrollment = await _enrollmentService.GetByIdAsync(id, userId, IsAdministrator());
        return enrollment is null || string.IsNullOrWhiteSpace(enrollment.PhotoUrl)
            ? NotFound()
            : Ok(new { url = enrollment.PhotoUrl });
    }

    [HttpPost("{id}/photo")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> UploadPhoto(string id, [FromForm] UploadMediaRequestDto request) => SavePhoto(id, request);

    [HttpPut("{id}/photo")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> UpdatePhoto(string id, [FromForm] UploadMediaRequestDto request) => SavePhoto(id, request);

    private async Task<IActionResult> SavePhoto(string id, UploadMediaRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var isAdministrator = IsAdministrator();
        var enrollment = await _enrollmentService.GetByIdAsync(id, userId, isAdministrator);
        if (enrollment is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("matricula", $"{enrollment.UserId}-{enrollment.CourseId}", enrollment.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/enrollments");
            var updatedEnrollment = await _enrollmentService.UpdatePhotoAsync(id, userId, isAdministrator, result.Url);
            return updatedEnrollment is null ? NotFound() : Ok(updatedEnrollment);
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
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        return await _enrollmentService.DeleteAsync(id, userId, IsAdministrator())
            ? NoContent()
            : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdministrator() => User.IsInRole(AccessLevel.Administrador.ToString());
}