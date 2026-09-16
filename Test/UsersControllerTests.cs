using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class UsersControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedUser()
    {
        var request = new CreateUserRequestDto("Aluno", "aluno@test.com", "senha", AccessLevel.Aluno);
        var response = new UserResponseDto(
            "user-id", "Aluno", "aluno@test.com", null, null, null, null,
            DateTime.UtcNow, DateTime.UtcNow, AccessLevel.Aluno);
        var service = new Mock<IUserService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync(response);
        var controller = new UsersController(service.Object);

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("user-id", ((UserResponseDto)created.Value!).Id);
    }

    [Fact]
    public async Task Delete_WhenUserExists_ReturnsNoContent()
    {
        var service = new Mock<IUserService>();
        service.Setup(value => value.DeleteAsync("user-id")).ReturnsAsync(true);
        var controller = new UsersController(service.Object);

        var result = await controller.Delete("user-id");

        Assert.IsType<NoContentResult>(result);
    }
}
