using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? search = null)
    {
        var categories = await _categoryService.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(search))
            categories = categories.Where(category => category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();
        return Ok(Utils.Paginate(categories, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(category);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _categoryService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}