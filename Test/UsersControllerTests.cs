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
    private static UserResponseDto Response() => new(
        "user-id", "Aluno", "aluno@test.com", null, null, null, null,
        DateTime.UtcNow, DateTime.UtcNow, AccessLevel.Aluno);

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var service = new Mock<IUserService>();
        service.Setup(value => value.GetAllAsync()).ReturnsAsync(new[] { Response() });

        var result = await new UsersController(service.Object).GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IUserService>();
        service.Setup(value => value.GetByIdAsync("missing")).ReturnsAsync((UserResponseDto?)null);

        var result = await new UsersController(service.Object).GetById("missing");

        Assert.IsType<NotFoundResult>(result);
    }

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

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var request = new UpdateUserRequestDto("Aluno", "aluno@test.com", AccessLevel.Aluno);
        var service = new Mock<IUserService>();
        service.Setup(value => value.UpdateAsync("user-id", request)).ReturnsAsync(Response());

        var result = await new UsersController(service.Object).Update("user-id", request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IUserService>();
        service.Setup(value => value.DeleteAsync("missing")).ReturnsAsync(false);

        var result = await new UsersController(service.Object).Delete("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}
