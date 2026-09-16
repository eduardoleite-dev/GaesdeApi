using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContentsController : ControllerBase
{
    private readonly IContentService _contentService;

    public ContentsController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? moduleId = null)
    {
        return Ok(await _contentService.GetAllAsync(moduleId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var content = await _contentService.GetByIdAsync(id);
        return content is null ? NotFound() : Ok(content);
    }

    [HttpPost]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Create(CreateContentRequestDto request)
    {
        var content = await _contentService.CreateAsync(request);
        return content is null
            ? Conflict(new { message = Messages.Contents.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = content.Id }, content);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Update(string id, UpdateContentRequestDto request)
    {
        var content = await _contentService.UpdateAsync(id, request);
        return content is null
            ? NotFound(new { message = Messages.Contents.UpdateNotFound })
            : Ok(content);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Professor,Administrador")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _contentService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}