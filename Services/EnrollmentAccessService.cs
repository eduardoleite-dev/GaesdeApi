using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class EnrollmentAccessService : IEnrollmentAccessService
{
    private readonly IMongoCollection<Enrollment> _enrollments;
    private readonly IMongoCollection<CourseModule> _modules;
    private readonly IMongoCollection<Quiz> _quizzes;
    private readonly IMongoCollection<Question> _questions;
    private readonly IMongoCollection<QuestionOption> _options;
    private readonly IMongoCollection<Content> _contents;

    public EnrollmentAccessService(IMongoDatabase database)
    {
        _enrollments = database.GetCollection<Enrollment>("Enrollments");
        _modules = database.GetCollection<CourseModule>("Modules");
        _quizzes = database.GetCollection<Quiz>("Quizzes");
        _questions = database.GetCollection<Question>("Questions");
        _options = database.GetCollection<QuestionOption>("QuestionOptions");
        _contents = database.GetCollection<Content>("Contents");
    }

    public async Task<bool> CanAccessCourseAsync(string userId, string courseId, AccessLevel accessLevel)
    {
        if (accessLevel != AccessLevel.Aluno)
            return true;

        return await _enrollments.Find(value =>
                value.UserId == userId && value.CourseId == courseId &&
                (value.Status == EnrollmentStatus.Active || value.Status == EnrollmentStatus.Completed) &&
                (value.ExpiresAt == null || value.ExpiresAt > DateTime.UtcNow))
            .Limit(1)
            .AnyAsync();
    }

    public async Task<bool> CanAccessModuleAsync(string userId, string moduleId, AccessLevel accessLevel)
    {
        var module = await _modules.Find(value => value.Id == moduleId).FirstOrDefaultAsync();
        return module is not null && await CanAccessCourseAsync(userId, module.CourseId, accessLevel);
    }

    public async Task<bool> CanAccessQuizAsync(string userId, string quizId, AccessLevel accessLevel)
    {
        var quiz = await _quizzes.Find(value => value.Id == quizId).FirstOrDefaultAsync();
        if (quiz is null)
            return false;
        return await CanAccessContentAsync(userId, quiz.ContentId, accessLevel);
    }

    public async Task<bool> CanAccessQuestionAsync(string userId, string questionId, AccessLevel accessLevel)
    {
        var question = await _questions.Find(value => value.Id == questionId).FirstOrDefaultAsync();
        return question is not null && await CanAccessQuizAsync(userId, question.QuizId, accessLevel);
    }

    public async Task<bool> CanAccessOptionAsync(string userId, string optionId, AccessLevel accessLevel)
    {
        var option = await _options.Find(value => value.Id == optionId).FirstOrDefaultAsync();
        return option is not null && await CanAccessQuestionAsync(userId, option.QuestionId, accessLevel);
    }

    private async Task<bool> CanAccessContentAsync(string userId, string contentId, AccessLevel accessLevel)
    {
        var content = await _contents
            .Find(value => value.Id == contentId)
            .FirstOrDefaultAsync();
        if (content is null)
            return false;
        return await CanAccessModuleAsync(userId, content.ModuleId, accessLevel);
    }
}
