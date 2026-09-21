using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICloudinaryService? _cloudinaryService;

    public UsersController(IUserService userService, ICloudinaryService? cloudinaryService = null)
    {
        _userService = userService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination, [FromQuery] string? search = null, [FromQuery] AccessLevel? accessLevel = null)
    {
        var users = await _userService.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(search))
            users = users.Where(user => user.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || user.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (accessLevel.HasValue)
            users = users.Where(user => user.AccessLevel == accessLevel.Value).ToArray();
        return Ok(Utils.Paginate(users, pagination));
    }

    [NonAction]
    public Task<IActionResult> GetAll() => GetAll(new PaginationRequest());

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null || string.IsNullOrWhiteSpace(user.AvatarUrl)
            ? NotFound()
            : Ok(new { url = user.AvatarUrl });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequestDto request)
    {
        var user = await _userService.CreateAsync(request);
        
        return user is null
            ? Conflict(new { message = Messages.Users.CreateConflict })
            : CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserRequestDto request)
    {
        var user = await _userService.UpdateAsync(id, request);

        return user is null
            ? NotFound(new { message = Messages.Users.UpdateNotFound })
            : Ok(user);
    }

    [HttpPost("{id}/photo")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(string id, [FromForm] UploadMediaRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Administrador") && currentUserId != id)
            return Forbid();

        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("usuario", user.Name, user.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/users");
            var updatedUser = await _userService.UpdateAvatarAsync(id, result.Url);
            return Ok(updatedUser);
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
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> UpdatePhoto(string id, [FromForm] UploadMediaRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Administrador") && currentUserId != id)
            return Forbid();

        var user = await _userService.GetByIdAsync(id);
        if (user is null)
            return NotFound();

        try
        {
            var publicId = CloudinaryService.CreatePublicId("usuario", user.Name, user.Id);
            var result = await _cloudinaryService!.UploadImageAsync(request.File!, publicId, "gaesde/users");
            var updatedUser = await _userService.UpdateAvatarAsync(id, result.Url);
            return Ok(updatedUser);
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
        return await _userService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}