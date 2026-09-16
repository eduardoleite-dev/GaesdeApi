using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class UserAnswersControllerTests
{
    private static UserAnswerResponseDto Response() => new(
        "answer-id", "attempt-id", "question-id", "option-id", null, null, true, 1, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_WithAttemptFilter_ReturnsOk()
    {
        var service = new Mock<IUserAnswerService>();
        service.Setup(value => value.GetAllAsync("attempt-id")).ReturnsAsync(new[] { Response() });

        Assert.IsType<OkObjectResult>(await new UserAnswersController(service.Object).GetAll("attempt-id"));
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateUserAnswerRequestDto("attempt-id", "question-id", "option-id");
        var service = new Mock<IUserAnswerService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync(Response());
        var controller = new UserAnswersController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        Assert.IsType<CreatedAtActionResult>(await controller.Create(request));
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        var request = new UpdateUserAnswerRequestDto(PointsEarned: 1);
        var service = new Mock<IUserAnswerService>();
        service.Setup(value => value.UpdateAsync("missing", request)).ReturnsAsync((UserAnswerResponseDto?)null);
        var controller = new UserAnswersController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        Assert.IsType<NotFoundObjectResult>(await controller.Update("missing", request));
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        var service = new Mock<IUserAnswerService>();
        service.Setup(value => value.DeleteAsync("answer-id")).ReturnsAsync(true);
        var controller = new UserAnswersController(service.Object);
        ControllerTestHelpers.SetUser(controller, "student-id", "Aluno");

        Assert.IsType<NoContentResult>(await controller.Delete("answer-id"));
    }
}