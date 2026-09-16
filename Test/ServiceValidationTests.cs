using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class ServiceValidationTests
{
    [Fact]
    public async Task UserService_WithInvalidAccessLevel_ReturnsNull()
    {
        var service = new UserService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(
            new CreateUserRequestDto("User", "user@test.com", "password", (AccessLevel)99));

        Assert.Null(result);
    }

    [Fact]
    public async Task CategoryService_WithBlankName_ReturnsNull()
    {
        var service = new CategoryService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(new CreateCategoryRequestDto("  "));

        Assert.Null(result);
    }

    [Fact]
    public async Task CourseService_WithInvalidTitle_ReturnsNull()
    {
        var service = new CourseService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(
            "teacher-id",
            new CreateCourseRequestDto("x", "course", CourseLevel.Beginner));

        Assert.Null(result);
    }

    [Fact]
    public async Task ContentService_WithInvalidVideoUrl_ReturnsNull()
    {
        var service = new ContentService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(
            new CreateContentRequestDto("module-id", "Video", ContentType.Video, 0, VideoUrl: "invalid"));

        Assert.Null(result);
    }

    [Fact]
    public async Task QuestionService_WithBlankQuizId_ReturnsNull()
    {
        var service = new QuestionService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(
            new CreateQuestionRequestDto("", QuestionType.Essay, "Answer"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ModuleService_WithInvalidOrder_ReturnsNull()
    {
        var service = new ModuleService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(
            "teacher-id", false, new CreateModuleRequestDto("course-id", "Module", -1));

        Assert.Null(result);
    }

    [Fact]
    public async Task EnrollmentService_WithBlankCourseId_ReturnsNull()
    {
        var service = new EnrollmentService(new Mock<IMongoDatabase>().Object);

        var result = await service.CreateAsync(
            "student-id", false, new CreateEnrollmentRequestDto(""));

        Assert.Null(result);
    }
}