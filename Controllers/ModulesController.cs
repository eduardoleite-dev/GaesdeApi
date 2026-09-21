using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModulesController : ControllerBase
{
    private readonly IModuleService _moduleService;
    private readonly IEnrollmentAccessService? _accessService;

    public ModulesController(IModuleService moduleService, IEnrollmentAccessService? accessService = null)
    {
        _moduleService = moduleService;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? courseId = null, [FromQuery] string? search = null)
    {
        var modules = await _moduleService.GetAllAsync(courseId);
        if (Utils.IsStudent(User))
        {
            if (string.IsNullOrWhiteSpace(courseId) ||
                _accessService is null || !await _accessService.CanAccessCourseAsync(Utils.GetUserId(User)!, courseId, AccessLevel.Aluno))
                return Forbid();
        }
        if (!string.IsNullOrWhiteSpace(search))
            modules = modules.Where(module => module.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();
        return Ok(Utils.Paginate(modules, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll(string? courseId) => GetAll(new PaginationRequest(), courseId);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var module = await _moduleService.GetByIdAsync(id);
        if (module is not null && Utils.IsStudent(User) &&
            (_accessService is null || !await _accessService.CanAccessModuleAsync(Utils.GetUserId(User)!, id, AccessLevel.Aluno)))
            return Forbid();
        return module is null ? NotFound() : Ok(module);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Create(CreateModuleRequestDto request)
    {
        var instructorId = GetUserId();
        if (instructorId is null)
            return Unauthorized();

        var module = await _moduleService.CreateAsync(instructorId, IsAdministrator(), request);
        return module is null
            ? Conflict(new { message = Messages.Modules.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = module.Id }, module);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Update(string id, UpdateModuleRequestDto request)
    {
        var instructorId = GetUserId();
        if (instructorId is null)
            return Unauthorized();

        var module = await _moduleService.UpdateAsync(id, instructorId, IsAdministrator(), request);
        return module is null
            ? NotFound(new { message = Messages.Modules.UpdateNotFound })
            : Ok(module);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Delete(string id)
    {
        var instructorId = GetUserId();
        if (instructorId is null)
            return Unauthorized();

        return await _moduleService.DeleteAsync(id, instructorId, IsAdministrator())
            ? NoContent()
            : NotFound();
    }

    private string? GetUserId() => Utils.GetUserId(User);

    private bool IsAdministrator() => Utils.IsAdministrator(User);
}