using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GaesdeApi.DTOs;
using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ICloudinaryService? _cloudinaryService;

    public CategoriesController(ICategoryService categoryService, ICloudinaryService? cloudinaryService = null)
    {
        _categoryService = categoryService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? search = null)
    {
        return Ok(await _categoryService.GetPageAsync(pagination, search));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category is null || string.IsNullOrWhiteSpace(category.ImageUrl)
            ? NotFound()
            : Ok(new { url = category.ImageUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequestDto request)
    {
        var category = await _categoryService.CreateAsync(request);
        return category is null
            ? Conflict(new { message = Messages.Categories.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateCategoryRequestDto request)
    {
        var category = await _categoryService.UpdateAsync(id, request);
        return category is null
            ? NotFound(new { message = Messages.Categories.UpdateNotFound })
            : Ok(category);
    }

    [HttpPost("{id}/photo")]
    [Authorize(Roles = "Administrador")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(string id, [FromForm] UploadMediaRequestDto request)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("categoria", category.Name, category.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/categories");
            var updatedCategory = await _categoryService.UpdateImageAsync(id, result.Url);
            return Ok(updatedCategory);
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

    [HttpPut("{id}/photo")]
    [Authorize(Roles = "Administrador")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UpdatePhoto(string id, [FromForm] UploadMediaRequestDto request)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("categoria", category.Name, category.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/categories");
            var updatedCategory = await _categoryService.UpdateImageAsync(id, result.Url);
            return Ok(updatedCategory);
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
        return await _categoryService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}