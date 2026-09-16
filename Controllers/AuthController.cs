using Microsoft.AspNetCore.Mvc;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

   [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResult = await _authService.AuthenticateAsync(request.Username, request.Password);
        
        if (authResult == null)
            return Unauthorized(new { message = Messages.Auth.InvalidCredentials });

        return Ok(authResult);
    }
}