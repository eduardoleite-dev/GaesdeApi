using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class QuizzesControllerTests
{
    private static QuizResponseDto Response() => new(
        "quiz-id", "content-id", 30, 60, 2, true, DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var service = new Mock<IQuizService>();
        service.Setup(value => value.GetAllAsync()).ReturnsAsync(new[] { Response() });

        Assert.IsType<OkObjectResult>(await new QuizzesController(service.Object).GetAll());
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateQuizRequestDto("content-id");
        var service = new Mock<IQuizService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync(Response());
        var controller = new QuizzesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        Assert.IsType<CreatedAtActionResult>(await controller.Create(request));
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        var request = new UpdateQuizRequestDto(60, 1, false);
        var service = new Mock<IQuizService>();
        service.Setup(value => value.UpdateAsync("missing", request)).ReturnsAsync((QuizResponseDto?)null);
        var controller = new QuizzesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        Assert.IsType<NotFoundObjectResult>(await controller.Update("missing", request));
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        var service = new Mock<IQuizService>();
        service.Setup(value => value.DeleteAsync("quiz-id")).ReturnsAsync(true);
        var controller = new QuizzesController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        Assert.IsType<NoContentResult>(await controller.Delete("quiz-id"));
    }

    [Fact]
    public async Task GetById_WhenStudentIsNotEnrolled_ReturnsForbid()
    {
        var service = new Mock<IQuizService>();
        service.Setup(value => value.GetByIdAsync("quiz-id")).ReturnsAsync(Response());
        var access = new Mock<IEnrollmentAccessService>();
        access.Setup(value => value.CanAccessQuizAsync("student-id", "quiz-id", AccessLevel.Aluno))
            .ReturnsAsync(false);
        var controller = new QuizzesController(service.Object, access.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.GetById("quiz-id");

        Assert.IsType<ForbidResult>(result);
    }
}