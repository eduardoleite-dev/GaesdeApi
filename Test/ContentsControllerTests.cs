using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class ContentsControllerTests
{
    private static ContentResponseDto Response() => new(
        "content-id", "module-id", "Video", ContentType.Video, 1, true, 120,
        "https://example.com/video.mp4", null, null, null, DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_PassesModuleFilter()
    {
        var service = new Mock<IContentService>();
        service.Setup(value => value.GetAllAsync("module-id")).ReturnsAsync(new[] { Response() });

        var result = await new ContentsController(service.Object).GetAll("module-id");

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.GetAllAsync("module-id"), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var service = new Mock<IContentService>();
        service.Setup(value => value.GetByIdAsync("content-id")).ReturnsAsync(Response());

        var result = await new ContentsController(service.Object).GetById("content-id");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhenRejected_ReturnsConflict()
    {
        var request = new CreateContentRequestDto("module-id", "Video", ContentType.Video, 0);
        var service = new Mock<IContentService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync((ContentResponseDto?)null);
        var controller = new ContentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Create(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        var service = new Mock<IContentService>();
        service.Setup(value => value.DeleteAsync("content-id")).ReturnsAsync(true);
        var controller = new ContentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Delete("content-id");

        Assert.IsType<NoContentResult>(result);
    }
}