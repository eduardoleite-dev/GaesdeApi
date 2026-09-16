using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class ModulesControllerTests
{
    private static ModuleResponseDto Response() => new(
        "module-id", "course-id", "Module", "Description", 1, DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_WithCourseFilter_ReturnsOk()
    {
        var service = new Mock<IModuleService>();
        service.Setup(value => value.GetAllAsync("course-id")).ReturnsAsync(new[] { Response() });

        var result = await new ModulesController(service.Object).GetAll("course-id");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_WithoutIdentity_ReturnsUnauthorized()
    {
        var controller = new ModulesController(new Mock<IModuleService>().Object);
        ControllerTestHelpers.SetUser(controller);

        var result = await controller.Create(new CreateModuleRequestDto("course-id", "Module", 0));

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_WhenServiceRejects_ReturnsConflict()
    {
        var request = new CreateModuleRequestDto("course-id", "Module", 0);
        var service = new Mock<IModuleService>();
        service.Setup(value => value.CreateAsync("teacher-id", false, request)).ReturnsAsync((ModuleResponseDto?)null);
        var controller = new ModulesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Create(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IModuleService>();
        service.Setup(value => value.DeleteAsync("missing", "teacher-id", false)).ReturnsAsync(false);
        var controller = new ModulesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Delete("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}