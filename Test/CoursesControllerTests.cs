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
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.GetByIdAsync("missing")).ReturnsAsync((CourseResponseDto?)null);
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.GetById("missing");

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

    [Fact]
    public async Task GetMine_WithProfessorIdentity_UsesProfessorScope()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.GetVisibleAsync("teacher-id", AccessLevel.Professor))
            .ReturnsAsync(new[] { Response() });
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.GetMine(new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.GetVisibleAsync("teacher-id", AccessLevel.Professor), Times.Once);
    }

    [Fact]
    public async Task GetCatalog_UsesPublishedCatalogScope()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.GetVisibleAsync("student-id", AccessLevel.Aluno))
            .ReturnsAsync(new[] { Response() });
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.GetCatalog(new PaginationRequest(), CourseLevel.Beginner, "Course");

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.GetVisibleAsync("student-id", AccessLevel.Aluno), Times.Once);
    }

    [Fact]
    public async Task GetCatalog_AsSellerUsesSellerScope()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.GetVisibleAsync("seller-id", AccessLevel.Vendedor))
            .ReturnsAsync(new[] { Response() });
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "seller-id", "Vendedor");

        var result = await controller.GetCatalog(new PaginationRequest());

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.GetVisibleAsync("seller-id", AccessLevel.Vendedor), Times.Once);
    }

    [Fact]
    public async Task SubmitForReview_UsesTokenInstructorId()
    {
        var service = new Mock<ICourseService>();
        service.Setup(value => value.SubmitForReviewAsync("course-id", "teacher-id"))
            .ReturnsAsync(Response());
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.SubmitForReview("course-id");

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.SubmitForReviewAsync("course-id", "teacher-id"), Times.Once);
    }

    [Fact]
    public async Task Update_AsAdministrator_PassesInstructorId()
    {
        var request = new UpdateCourseRequestDto(
            "Course", "course", CourseLevel.Beginner, 10,
            InstructorId: "professor-id");
        var service = new Mock<ICourseService>();
        service.Setup(value => value.UpdateAsync("course-id", "admin-id", true, request))
            .ReturnsAsync(Response());
        var controller = new CoursesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "admin-id", "Administrador");

        var result = await controller.Update("course-id", request);

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.UpdateAsync("course-id", "admin-id", true, request), Times.Once);
    }
}