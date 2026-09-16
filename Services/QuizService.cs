using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class QuizService : IQuizService
{
    private readonly IMongoCollection<Quiz> _quizzesCollection;
    private readonly IMongoCollection<Content> _contentsCollection;

    public QuizService(IMongoDatabase database)
    {
        _quizzesCollection = database.GetCollection<Quiz>("Quizzes");
        _contentsCollection = database.GetCollection<Content>("Contents");
    }

    public async Task<IReadOnlyCollection<QuizResponseDto>> GetAllAsync()
    {
        var quizzes = await _quizzesCollection
            .Find(Builders<Quiz>.Filter.Empty)
            .SortBy(quiz => quiz.CreatedAt)
            .ToListAsync();

        return quizzes.Select(ToResponse).ToArray();
    }

    public async Task<QuizResponseDto?> GetByIdAsync(string id)
    {
        var quiz = await _quizzesCollection
            .Find(existingQuiz => existingQuiz.Id == id)
            .FirstOrDefaultAsync();

        return quiz is null ? null : ToResponse(quiz);
    }

    public async Task<QuizResponseDto?> CreateAsync(CreateQuizRequestDto request)
    {
        if (!IsValid(request.ContentId, request.PassingScorePercentage, request.AttemptsAllowed,
                request.TimeLimitMinutes) ||
            !await IsQuizContentAsync(request.ContentId) ||
            await QuizExistsForContentAsync(request.ContentId))
            return null;

        var now = DateTime.UtcNow;
        var quiz = new Quiz
        {
            ContentId = request.ContentId.Trim(),
            PassingScorePercentage = request.PassingScorePercentage,
            AttemptsAllowed = request.AttemptsAllowed,
            ShuffleQuestions = request.ShuffleQuestions,
            TimeLimitMinutes = request.TimeLimitMinutes,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _quizzesCollection.InsertOneAsync(quiz);
        return ToResponse(quiz);
    }

    public async Task<QuizResponseDto?> UpdateAsync(string id, UpdateQuizRequestDto request)
    {
        if (!IsValid("valid", request.PassingScorePercentage, request.AttemptsAllowed, request.TimeLimitMinutes))
            return null;

        var quiz = await _quizzesCollection
            .Find(existingQuiz => existingQuiz.Id == id)
            .FirstOrDefaultAsync();

        if (quiz is null)
            return null;

        quiz.PassingScorePercentage = request.PassingScorePercentage;
        quiz.AttemptsAllowed = request.AttemptsAllowed;
        quiz.ShuffleQuestions = request.ShuffleQuestions;
        quiz.TimeLimitMinutes = request.TimeLimitMinutes;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizzesCollection.ReplaceOneAsync(existingQuiz => existingQuiz.Id == id, quiz);
        return ToResponse(quiz);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _quizzesCollection.DeleteOneAsync(quiz => quiz.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> IsQuizContentAsync(string contentId)
    {
        return await _contentsCollection.Find(content =>
                content.Id == contentId && content.Type == ContentType.Quiz)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> QuizExistsForContentAsync(string contentId)
    {
        return await _quizzesCollection.Find(quiz => quiz.ContentId == contentId)
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(string contentId, decimal passingScore, int attemptsAllowed, int? timeLimitMinutes)
    {
        return !string.IsNullOrWhiteSpace(contentId) &&
            passingScore is >= 0 and <= 100 &&
            attemptsAllowed >= 1 &&
            timeLimitMinutes is null or > 0;
    }

    private static QuizResponseDto ToResponse(Quiz quiz) => new(
        quiz.Id,
        quiz.ContentId,
        quiz.TimeLimitMinutes,
        quiz.PassingScorePercentage,
        quiz.AttemptsAllowed,
        quiz.ShuffleQuestions,
        quiz.CreatedAt,
        quiz.UpdatedAt);
}