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
public class ContentsController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly ICloudinaryService _cloudinaryService;

    public ContentsController(IContentService contentService, ICloudinaryService cloudinaryService)
    {
        _contentService = contentService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? moduleId = null, [FromQuery] ContentType? type = null, [FromQuery] bool? freePreview = null)
    {
        var contents = await _contentService.GetAllAsync(moduleId);
        if (type.HasValue)
            contents = contents.Where(content => content.Type == type.Value).ToArray();
        if (freePreview.HasValue)
            contents = contents.Where(content => content.IsFreePreview == freePreview.Value).ToArray();
        return Ok(Utils.Paginate(contents, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll(string? moduleId) => GetAll(new PaginationRequest(), moduleId);

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

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(string id)
    {
        var content = await _contentService.GetByIdAsync(id);
        return content is null || string.IsNullOrWhiteSpace(content.PhotoUrl)
            ? NotFound()
            : Ok(new { url = content.PhotoUrl });
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
        var content = await _contentService.GetByIdAsync(id);
        if (content is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("conteudo", content.Title, content.Id);
            var result = await _cloudinaryService.UploadImageAsync(request.File!, publicId, "gaesde/contents");
            var updatedContent = await _contentService.UpdatePhotoAsync(id, result.Url);
            return updatedContent is null ? NotFound() : Ok(updatedContent);
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
        return await _contentService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}