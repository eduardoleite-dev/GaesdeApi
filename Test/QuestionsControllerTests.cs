using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class QuestionsControllerTests
{
    private static QuestionResponseDto Response() => new(
        "question-id", "quiz-id", QuestionType.MultipleChoice, "Question?", 1, 0,
        DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_PassesQuizFilter()
    {
        var service = new Mock<IQuestionService>();
        service.Setup(value => value.GetAllAsync("quiz-id")).ReturnsAsync(new[] { Response() });

        var result = await new QuestionsController(service.Object).GetAll("quiz-id");

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateQuestionRequestDto("quiz-id", QuestionType.TrueFalse, "Is this true?");
        var service = new Mock<IQuestionService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync(Response());
        var controller = new QuestionsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        var request = new UpdateQuestionRequestDto(QuestionType.Essay, "Write an answer", 2, 1);
        var service = new Mock<IQuestionService>();
        service.Setup(value => value.UpdateAsync("missing", request)).ReturnsAsync((QuestionResponseDto?)null);
        var controller = new QuestionsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Update("missing", request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<IQuestionService>();
        service.Setup(value => value.DeleteAsync("missing")).ReturnsAsync(false);
        var controller = new QuestionsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "teacher-id", "Professor");

        var result = await controller.Delete("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}