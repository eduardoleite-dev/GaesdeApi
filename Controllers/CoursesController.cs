using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ICloudinaryService? _cloudinaryService;

    public CoursesController(ICourseService courseService, ICloudinaryService? cloudinaryService = null)
    {
        _courseService = courseService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] CourseStatus? status = null, [FromQuery] CourseLevel? level = null, [FromQuery] string? search = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var courses = await _courseService.GetVisibleAsync(userId, GetAccessLevel());
        return Ok(Utils.Paginate(FilterCourses(courses, status, level, search), pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("mine")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetMine([FromQuery] PaginationRequest pagination, [FromQuery] CourseStatus? status = null, [FromQuery] CourseLevel? level = null, [FromQuery] string? search = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var courses = await _courseService.GetVisibleAsync(userId, AccessLevel.Professor);
        return Ok(Utils.Paginate(FilterCourses(courses, status, level, search), pagination));
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] PaginationRequest pagination, [FromQuery] CourseLevel? level = null, [FromQuery] string? search = null)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        var accessLevel = GetAccessLevel();
        var courses = await _courseService.GetVisibleAsync(userId, accessLevel);
        return Ok(Utils.Paginate(FilterCourses(courses, CourseStatus.Published, level, search), pagination));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var course = await _courseService.GetVisibleByIdAsync(id, userId, GetAccessLevel());
        return course is null ? NotFound() : Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Create(CreateCourseRequestDto request)
    {
        var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (instructorId is null)
            return Unauthorized();

        var course = await _courseService.CreateAsync(instructorId, request);
        return course is null
            ? Conflict(new { message = Messages.Courses.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Update(string id, UpdateCourseRequestDto request)
    {
        var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (instructorId is null)
            return Unauthorized();

        var course = await _courseService.UpdateAsync(
            id,
            instructorId,
            User.IsInRole("Administrador"),
            request);
        return course is null
            ? NotFound(new { message = Messages.Courses.UpdateNotFound })
            : Ok(course);
    }

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(string id)
    {
        var course = await _courseService.GetByIdAsync(id);
        return course is null || string.IsNullOrWhiteSpace(course.CoverImage)
            ? NotFound()
            : Ok(new { url = course.CoverImage });
    }

    [HttpPost("{id}/photo")]
    [Authorize(Roles = "Professor,Administrador")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> CreatePhoto(string id, [FromForm] UploadMediaRequestDto request) => SavePhoto(id, request);

    [HttpPut("{id}/photo")]
    [Authorize(Roles = "Professor,Administrador")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public Task<IActionResult> UpdatePhoto(string id, [FromForm] UploadMediaRequestDto request) => SavePhoto(id, request);

    private async Task<IActionResult> SavePhoto(string id, UploadMediaRequestDto request)
    {
        var userId = GetUserId();
        var course = await _courseService.GetByIdAsync(id);
        if (course is null)
            return NotFound();
        if (!User.IsInRole("Administrador") && course.InstructorId != userId)
            return Forbid();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("curso", course.Title, course.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/courses");
            var updatedCourse = await _courseService.UpdateCoverImageAsync(id, result.Url);
            return updatedCourse is null ? NotFound() : Ok(updatedCourse);
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

    [HttpPatch("{id}/publish")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Publish(string id)
    {
        var course = await _courseService.PublishAsync(id);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpPatch("{id}/submit-review")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> SubmitForReview(string id)
    {
        var instructorId = GetUserId();
        if (instructorId is null)
            return Unauthorized();

        var course = await _courseService.SubmitForReviewAsync(id, instructorId);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpPatch("{id}/archive")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Archive(string id)
    {
        var course = await _courseService.ArchiveAsync(id);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (instructorId is null)
            return Unauthorized();

        var isAdministrator = User.IsInRole("Administrador");
        return await _courseService.DeleteAsync(id, instructorId, isAdministrator)
            ? NoContent()
            : NotFound();
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private AccessLevel GetAccessLevel() => Utils.GetAccessLevel(User);

    private static IEnumerable<CourseResponseDto> FilterCourses(
        IEnumerable<CourseResponseDto> courses,
        CourseStatus? status,
        CourseLevel? level,
        string? search)
    {
        if (status.HasValue)
            courses = courses.Where(course => course.Status == status.Value);
        if (level.HasValue)
            courses = courses.Where(course => course.Level == level.Value);
        if (!string.IsNullOrWhiteSpace(search))
            courses = courses.Where(course => course.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
        return courses;
    }
}