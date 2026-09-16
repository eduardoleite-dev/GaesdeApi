using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GaesdeApi.DTOs;
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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _userService.GetAllAsync());
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
            ? Conflict(new { message = "E-mail já cadastrado ou nível de acesso inválido." })
            : CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserRequestDto request)
    {
        var user = await _userService.UpdateAsync(id, request);
        return user is null
            ? NotFound(new { message = "Usuário não encontrado, e-mail já cadastrado ou nível de acesso inválido." })
            : Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        return await _userService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}