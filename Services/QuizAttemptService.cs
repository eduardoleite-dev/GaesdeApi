using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class QuizAttemptService : IQuizAttemptService
{
    private readonly IMongoCollection<QuizAttempt> _attempts;
    private readonly IMongoCollection<Quiz> _quizzes;
    private readonly IMongoCollection<Content> _contents;
    private readonly IMongoCollection<CourseModule> _modules;
    private readonly IMongoCollection<Enrollment> _enrollments;
    private readonly IMongoCollection<UserAnswer> _answers;
    private readonly IMongoCollection<Question> _questions;

    public QuizAttemptService(IMongoDatabase database)
    {
        _attempts = database.GetCollection<QuizAttempt>("QuizAttempts");
        _quizzes = database.GetCollection<Quiz>("Quizzes");
        _contents = database.GetCollection<Content>("Contents");
        _modules = database.GetCollection<CourseModule>("Modules");
        _enrollments = database.GetCollection<Enrollment>("Enrollments");
        _answers = database.GetCollection<UserAnswer>("UserAnswers");
        _questions = database.GetCollection<Question>("Questions");
    }

    public async Task<QuizAttemptResponseDto?> StartAsync(string userId, StartQuizAttemptRequestDto request)
    {
        var quiz = await _quizzes.Find(value => value.Id == request.QuizId).FirstOrDefaultAsync();
        var enrollment = await _enrollments.Find(value =>
                value.Id == request.EnrollmentId && value.UserId == userId &&
                value.Status == EnrollmentStatus.Active &&
                (value.ExpiresAt == null || value.ExpiresAt > DateTime.UtcNow))
            .FirstOrDefaultAsync();
        if (quiz is null || enrollment is null || !await QuizBelongsToCourseAsync(quiz, enrollment.CourseId))
            return null;

        var existing = await _attempts.Find(value =>
                value.QuizId == quiz.Id && value.UserId == userId &&
                value.Status == QuizAttemptStatus.InProgress)
            .FirstOrDefaultAsync();
        if (existing is not null)
            return ToResponse(existing);

        var attempts = await _attempts.CountDocumentsAsync(value =>
            value.QuizId == quiz.Id && value.UserId == userId);
        if (attempts >= quiz.AttemptsAllowed)
            return null;

        var attempt = new QuizAttempt
        {
            QuizId = quiz.Id,
            UserId = userId,
            EnrollmentId = enrollment.Id,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };
        await _attempts.InsertOneAsync(attempt);
        return ToResponse(attempt);
    }

    public async Task<IReadOnlyCollection<QuizAttemptResponseDto>> GetAllAsync(
        string userId,
        bool isAdministrator,
        string? quizId = null)
    {
        var filter = isAdministrator
            ? Builders<QuizAttempt>.Filter.Empty
            : Builders<QuizAttempt>.Filter.Eq(value => value.UserId, userId);
        if (!string.IsNullOrWhiteSpace(quizId))
            filter &= Builders<QuizAttempt>.Filter.Eq(value => value.QuizId, quizId);

        var attempts = await _attempts.Find(filter)
            .SortByDescending(value => value.StartedAt)
            .ToListAsync();
        return attempts.Select(ToResponse).ToArray();
    }

    public async Task<QuizAttemptResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<QuizAttempt>.Filter.Eq(value => value.Id, id);
        if (!isAdministrator)
            filter &= Builders<QuizAttempt>.Filter.Eq(value => value.UserId, userId);
        var attempt = await _attempts.Find(filter).FirstOrDefaultAsync();
        return attempt is null ? null : ToResponse(attempt);
    }

    public async Task<QuizAttemptResponseDto?> FinishAsync(string id, string userId, bool isAdministrator)
    {
        var attempt = await FindAuthorizedAsync(id, userId, isAdministrator);
        if (attempt is null || attempt.Status != QuizAttemptStatus.InProgress)
            return null;

        var quiz = await _quizzes.Find(value => value.Id == attempt.QuizId).FirstOrDefaultAsync();
        if (quiz is null)
            return null;

        var questions = await _questions.Find(value => value.QuizId == quiz.Id).ToListAsync();
        var answers = await _answers.Find(value => value.AttemptId == attempt.Id).ToListAsync();
        var totalPoints = questions.Sum(value => value.Points);
        var earnedPoints = answers.Sum(value => value.PointsEarned);
        var score = totalPoints <= 0 ? 0 : Math.Round(earnedPoints * 100m / totalPoints, 2);

        attempt.Status = QuizAttemptStatus.Finished;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TotalScore = score;
        attempt.IsPassed = score >= quiz.PassingScorePercentage;
        await _attempts.ReplaceOneAsync(value => value.Id == id, attempt);
        return ToResponse(attempt);
    }

    public async Task<QuizAttemptResponseDto?> AbandonAsync(string id, string userId, bool isAdministrator)
    {
        var attempt = await FindAuthorizedAsync(id, userId, isAdministrator);
        if (attempt is null || attempt.Status == QuizAttemptStatus.Finished)
            return null;
        attempt.Status = QuizAttemptStatus.Abandoned;
        await _attempts.ReplaceOneAsync(value => value.Id == id, attempt);
        return ToResponse(attempt);
    }

    private async Task<QuizAttempt?> FindAuthorizedAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<QuizAttempt>.Filter.Eq(value => value.Id, id);
        if (!isAdministrator)
            filter &= Builders<QuizAttempt>.Filter.Eq(value => value.UserId, userId);
        return await _attempts.Find(filter).FirstOrDefaultAsync();
    }

    private async Task<bool> QuizBelongsToCourseAsync(Quiz quiz, string courseId)
    {
        var content = await _contents.Find(value => value.Id == quiz.ContentId).FirstOrDefaultAsync();
        if (content is null)
            return false;
        var module = await _modules.Find(value => value.Id == content.ModuleId).FirstOrDefaultAsync();
        return module is not null && module.CourseId == courseId;
    }

    private static QuizAttemptResponseDto ToResponse(QuizAttempt attempt) => new(
        attempt.Id,
        attempt.QuizId,
        attempt.UserId,
        attempt.EnrollmentId,
        attempt.Status,
        attempt.StartedAt,
        attempt.SubmittedAt,
        attempt.TotalScore,
        attempt.IsPassed);
}
