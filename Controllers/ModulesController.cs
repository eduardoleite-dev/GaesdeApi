using System.Security.Claims;
using GaesdeApi.DTOs;
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

    public ModulesController(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? courseId = null)
    {
        return Ok(await _moduleService.GetAllAsync(courseId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var module = await _moduleService.GetByIdAsync(id);
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

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdministrator() => User.IsInRole("Administrador");
}