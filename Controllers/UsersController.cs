using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _userService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}