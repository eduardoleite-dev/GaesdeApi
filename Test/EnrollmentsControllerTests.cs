using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class EnrollmentsControllerTests
{
    private static EnrollmentResponseDto Response() => new(
        "enrollment-id", "student-id", "course-id", null, EnrollmentStatus.Active, 25,
        DateTime.UtcNow, null, DateTime.UtcNow, true, false, false);

    [Fact]
    public async Task GetAll_WithoutIdentity_ReturnsUnauthorized()
    {
        var controller = new EnrollmentsController(new Mock<IEnrollmentService>().Object);
        ControllerTestHelpers.SetUser(controller);

        var result = await controller.GetAll();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetAll_AsAdministratorPassesAdministratorFlag()
    {
        var service = new Mock<IEnrollmentService>();
        service.Setup(value => value.GetAllAsync("admin-id", true)).ReturnsAsync(new[] { Response() });
        var controller = new EnrollmentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "admin-id", "Administrador");

        var result = await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.GetAllAsync("admin-id", true), Times.Once);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateEnrollmentRequestDto("course-id");
        var service = new Mock<IEnrollmentService>();
        service.Setup(value => value.CreateAsync("student-id", false, request)).ReturnsAsync(Response());
        var controller = new EnrollmentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateProgress_WhenInvalid_ReturnsBadRequest()
    {
        var request = new UpdateEnrollmentProgressRequestDto(150);
        var service = new Mock<IEnrollmentService>();
        service.Setup(value => value.UpdateProgressAsync("enrollment-id", "student-id", false, 150))
            .ReturnsAsync((EnrollmentResponseDto?)null);
        var controller = new EnrollmentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.UpdateProgress("enrollment-id", request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_WhenFound_ReturnsOk()
    {
        var service = new Mock<IEnrollmentService>();
        service.Setup(value => value.UpdateStatusAsync("enrollment-id", "student-id", false, EnrollmentStatus.Completed))
            .ReturnsAsync(Response());
        var controller = new EnrollmentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.UpdateStatus("enrollment-id", EnrollmentStatus.Completed);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetMine_UsesTokenUserIdAndPaginates()
    {
        var service = new Mock<IEnrollmentService>();
        service.Setup(value => value.GetAllAsync("student-id", false)).ReturnsAsync(new[] { Response() });
        var controller = new EnrollmentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.GetMine(new PaginationRequest { Page = 1, PageSize = 5 });

        Assert.IsType<OkObjectResult>(result);
        service.Verify(value => value.GetAllAsync("student-id", false), Times.Once);
    }
}