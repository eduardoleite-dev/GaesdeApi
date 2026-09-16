using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class CoursesControllerTests
{
    private static CourseResponseDto Response() => new(
        "course-id", "Course", "course", "Description", null, 10,
        CourseStatus.Draft, CourseLevel.Beginner, "teacher-id", null, null,
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.GetAllAsync()).ReturnsAsync(new[] { Response() });

        var result = await new CoursesController(service.Object).GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.GetByIdAsync("missing")).ReturnsAsync((CourseResponseDto?)null);

        var result = await new CoursesController(service.Object).GetById("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithoutIdentity_ReturnsUnauthorized()
    {
        var controller = new CoursesController(new Mock<ICourseService>().Object);
        ControllerTestHelpers.SetUser(controller);

        var result = await controller.Create(new CreateCourseRequestDto("Course", "course", CourseLevel.Beginner));

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateCourseRequestDto("Course", "course", CourseLevel.Beginner);
        var service = new Mock<ICourseService>();
        service.Setup(value => value.CreateAsync("teacher-id", request)).ReturnsAsync(Response());
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Publish_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.PublishAsync("missing")).ReturnsAsync((CourseResponseDto?)null);

        var result = await new CoursesController(service.Object).Publish("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_AsAdministrator_WhenSuccessful_ReturnsNoContent()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.DeleteAsync("course-id", "admin-id", true)).ReturnsAsync(true);
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "admin-id", "Administrador");

        var result = await controller.Delete("course-id");

        Assert.IsType<NoContentResult>(result);
    }
}