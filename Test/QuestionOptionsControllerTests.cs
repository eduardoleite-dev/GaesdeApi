using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class QuestionOptionsControllerTests
{
    private static QuestionOptionResponseDto Response() => new(
        "option-id", "question-id", "Correct", true, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_WithQuestionFilter_ReturnsOk()
    {
        var service = new Mock<IQuestionOptionService>();
        service.Setup(value => value.GetAllAsync("question-id")).ReturnsAsync(new[] { Response() });

        Assert.IsType<OkObjectResult>(await new QuestionOptionsController(service.Object).GetAll("question-id"));
    }

    [Fact]
    public async Task Create_WhenRejected_ReturnsConflict()
    {
        var request = new CreateQuestionOptionRequestDto("question-id", "Option");
        var service = new Mock<IQuestionOptionService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync((QuestionOptionResponseDto?)null);
        var controller = new QuestionOptionsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        Assert.IsType<ConflictObjectResult>(await controller.Create(request));
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var request = new UpdateQuestionOptionRequestDto("Updated", true);
        var service = new Mock<IQuestionOptionService>();
        service.Setup(value => value.UpdateAsync("option-id", request)).ReturnsAsync(Response());
        var controller = new QuestionOptionsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        Assert.IsType<OkObjectResult>(await controller.Update("option-id", request));
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IQuestionOptionService>();
        service.Setup(value => value.DeleteAsync("missing")).ReturnsAsync(false);
        var controller = new QuestionOptionsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        Assert.IsType<NotFoundResult>(await controller.Delete("missing"));
    }
}