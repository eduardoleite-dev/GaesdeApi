using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var request = new LoginRequest { Username = "user", Password = "password" };
        var response = new LoginResponseDto("token", DateTime.UtcNow.AddHours(1), "user", "id");
        var service = new Mock<IAuthService>();
        service.Setup(value => value.AuthenticateAsync("user", "password")).ReturnsAsync(response);

        var result = await new AuthController(service.Object).Login(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var service = new Mock<IAuthService>();
        service.Setup(value => value.AuthenticateAsync("user", "bad")).ReturnsAsync((LoginResponseDto?)null);

        var result = await new AuthController(service.Object).Login(
            new LoginRequest { Username = "user", Password = "bad" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}