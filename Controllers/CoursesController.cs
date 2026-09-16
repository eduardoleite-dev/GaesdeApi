using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _courseService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var course = await _courseService.GetByIdAsync(id);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Professor")]
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
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> Update(string id, UpdateCourseRequestDto request)
    {
        var instructorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (instructorId is null)
            return Unauthorized();

        var course = await _courseService.UpdateAsync(id, instructorId, request);
        return course is null
            ? NotFound(new { message = Messages.Courses.UpdateNotFound })
            : Ok(course);
    }

    [HttpPatch("{id}/publish")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Publish(string id)
    {
        var course = await _courseService.PublishAsync(id);
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
}