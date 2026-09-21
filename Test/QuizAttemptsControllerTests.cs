using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class QuizAttemptsControllerTests
{
    private static QuizAttemptResponseDto Response() => new(
        "attempt-id", "quiz-id", "student-id", "enrollment-id",
        QuizAttemptStatus.InProgress, DateTime.UtcNow, null, null, null);

    [Fact]
    public async Task Start_WithoutIdentity_ReturnsUnauthorized()
    {
        var controller = new QuizAttemptsController(new Mock<IQuizAttemptService>().Object);
        ControllerTestHelpers.SetUser(controller);

        var result = await controller.Start(new StartQuizAttemptRequestDto("quiz-id", "enrollment-id"));

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Start_WhenValid_ReturnsCreated()
    {
        var request = new StartQuizAttemptRequestDto("quiz-id", "enrollment-id");
        var service = new Mock<IQuizAttemptService>();
        service.Setup(value => value.StartAsync("student-id", request)).ReturnsAsync(Response());
        var controller = new QuizAttemptsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.Start(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Finish_WhenMissing_ReturnsBadRequest()
    {
        var service = new Mock<IQuizAttemptService>();
        service.Setup(value => value.FinishAsync("missing", "student-id", false))
            .ReturnsAsync((QuizAttemptResponseDto?)null);
        var controller = new QuizAttemptsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        var result = await controller.Finish("missing");

        Assert.IsType<BadRequestResult>(result);
    }
}
