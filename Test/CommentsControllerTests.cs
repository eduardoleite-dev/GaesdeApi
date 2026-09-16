using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class CommentsControllerTests
{
    private static CommentResponseDto Response() => new(
        "comment-id", CommentType.Chat, "Hello", "author-id", null,
        new[] { "recipient-id" }, null, Array.Empty<CommentAttachmentDto>(),
        Array.Empty<CommentReaction>(), Array.Empty<string>(), DateTime.UtcNow, DateTime.UtcNow, null);

    [Fact]
    public async Task GetAll_WithoutIdentity_ReturnsUnauthorized()
    {
        var controller = new CommentsController(new Mock<ICommentService>().Object);
        ControllerTestHelpers.SetUser(controller);

        Assert.IsType<UnauthorizedResult>(await controller.GetAll());
    }

    [Fact]
    public async Task Create_WhenValid_ReturnsCreated()
    {
        var request = new CreateCommentRequestDto(CommentType.Chat, "Hello", new[] { "recipient-id" });
        var service = new Mock<ICommentService>();
        service.Setup(value => value.CreateAsync("author-id", request)).ReturnsAsync(Response());
        var controller = new CommentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "author-id", "Aluno");

        Assert.IsType<CreatedAtActionResult>(await controller.Create(request));
    }

    [Fact]
    public async Task AddReaction_WhenFound_ReturnsOk()
    {
        var request = new CommentReactionRequestDto("👍");
        var service = new Mock<ICommentService>();
        service.Setup(value => value.AddReactionAsync("comment-id", "user-id", request)).ReturnsAsync(Response());
        var controller = new CommentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "user-id", "Aluno");

        Assert.IsType<OkObjectResult>(await controller.AddReaction("comment-id", request));
    }

    [Fact]
    public async Task Archive_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICommentService>();
        service.Setup(value => value.ArchiveAsync("missing", "user-id")).ReturnsAsync((CommentResponseDto?)null);
        var controller = new CommentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "user-id", "Aluno");

        Assert.IsType<NotFoundResult>(await controller.Archive("missing"));
    }

    [Fact]
    public async Task Delete_AsAdministrator_ReturnsNoContent()
    {
        var service = new Mock<ICommentService>();
        service.Setup(value => value.DeleteAsync("comment-id", "admin-id", true)).ReturnsAsync(true);
        var controller = new CommentsController(service.Object);
        ControllerTestHelpers.SetUser(controller, "admin-id", "Administrador");

        Assert.IsType<NoContentResult>(await controller.Delete("comment-id"));
    }
}