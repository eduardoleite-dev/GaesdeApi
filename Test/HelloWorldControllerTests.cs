using GaesdeApi.Controllers;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace GaesdeApi.Tests;

public class HelloWorldControllerTests
{
    [Fact]
    public void GetPublic_ReturnsHelloWorld()
    {
        var controller = new HelloWorldController(new Mock<IMongoTestService>().Object);

        var result = controller.GetPublic();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void GetPrivate_ReturnsMongoStatus()
    {
        var service = new Mock<IMongoTestService>();
        service.Setup(value => value.TestConnection()).Returns("connected");
        var controller = new HelloWorldController(service.Object);

        var result = controller.GetPrivate();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetAdministrator_WithWrongAccessLevel_ReturnsForbidden()
    {
        var controller = new HelloWorldController(new Mock<IMongoTestService>().Object);
        ControllerTestHelpers.SetUser(controller);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("nivel_acesso", "3") }, "Test"));

        var result = controller.GetAdministrator();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void GetStudent_WithCorrectAccessLevel_ReturnsOk()
    {
        var controller = new HelloWorldController(new Mock<IMongoTestService>().Object);
        ControllerTestHelpers.SetUser(controller);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("nivel_acesso", "3") }, "Test"));

        var result = controller.GetStudent();

        Assert.IsType<OkObjectResult>(result);
    }
}