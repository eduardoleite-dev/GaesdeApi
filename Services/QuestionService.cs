using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class QuestionService : IQuestionService
{
    private readonly IMongoCollection<Question> _questionsCollection;
    private readonly IMongoCollection<Quiz> _quizzesCollection;

    public QuestionService(IMongoDatabase database)
    {
        _questionsCollection = database.GetCollection<Question>("Questions");
        _quizzesCollection = database.GetCollection<Quiz>("Quizzes");
    }

    public async Task<IReadOnlyCollection<QuestionResponseDto>> GetAllAsync(string? quizId = null)
    {
        var filter = string.IsNullOrWhiteSpace(quizId)
            ? Builders<Question>.Filter.Empty
            : Builders<Question>.Filter.Eq(question => question.QuizId, quizId);

        var questions = await _questionsCollection
            .Find(filter)
            .SortBy(question => question.OrderIndex)
            .ToListAsync();

        return questions.Select(ToResponse).ToArray();
    }

    public async Task<QuestionResponseDto?> GetByIdAsync(string id)
    {
        var question = await _questionsCollection
            .Find(existingQuestion => existingQuestion.Id == id)
            .FirstOrDefaultAsync();

        return question is null ? null : ToResponse(question);
    }

    public async Task<QuestionResponseDto?> CreateAsync(CreateQuestionRequestDto request)
    {
        if (!IsValid(request.QuizId, request.Type, request.QuestionText, request.Points, request.OrderIndex) ||
            !await IsQuizAsync(request.QuizId) ||
            await OrderExistsAsync(request.QuizId, request.OrderIndex))
            return null;

        var now = DateTime.UtcNow;
        var question = new Question
        {
            QuizId = request.QuizId.Trim(),
            Type = request.Type,
            QuestionText = request.QuestionText.Trim(),
            Points = request.Points,
            OrderIndex = request.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _questionsCollection.InsertOneAsync(question);
        return ToResponse(question);
    }

    public async Task<QuestionResponseDto?> UpdateAsync(string id, UpdateQuestionRequestDto request)
    {
        if (!IsValid("valid", request.Type, request.QuestionText, request.Points, request.OrderIndex))
            return null;

        var question = await _questionsCollection
            .Find(existingQuestion => existingQuestion.Id == id)
            .FirstOrDefaultAsync();

        if (question is null || await OrderExistsAsync(question.QuizId, request.OrderIndex, id))
            return null;

        question.Type = request.Type;
        question.QuestionText = request.QuestionText.Trim();
        question.Points = request.Points;
        question.OrderIndex = request.OrderIndex;
        question.UpdatedAt = DateTime.UtcNow;

        await _questionsCollection.ReplaceOneAsync(existingQuestion => existingQuestion.Id == id, question);
        return ToResponse(question);
    }

    public async Task<QuestionResponseDto?> UpdatePhotoAsync(string id, string photoUrl)
    {
        var question = await _questionsCollection
            .Find(existingQuestion => existingQuestion.Id == id)
            .FirstOrDefaultAsync();

        if (question is null)
            return null;

        question.PhotoUrl = photoUrl;
        question.UpdatedAt = DateTime.UtcNow;
        await _questionsCollection.ReplaceOneAsync(existingQuestion => existingQuestion.Id == id, question);
        return ToResponse(question);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _questionsCollection.DeleteOneAsync(question => question.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> IsQuizAsync(string quizId)
    {
        return await _quizzesCollection.Find(quiz => quiz.Id == quizId)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> OrderExistsAsync(string quizId, int orderIndex, string? excludedId = null)
    {
        return await _questionsCollection.Find(question =>
                question.QuizId == quizId &&
                question.OrderIndex == orderIndex &&
                (excludedId == null || question.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(
        string quizId,
        QuestionType type,
        string questionText,
        decimal points,
        int orderIndex)
    {
        return !string.IsNullOrWhiteSpace(quizId) &&
            Enum.IsDefined(type) &&
            !string.IsNullOrWhiteSpace(questionText) &&
            points >= 0 &&
            orderIndex >= 0;
    }

    private static QuestionResponseDto ToResponse(Question question) => new(
        question.Id,
        question.QuizId,
        question.Type,
        question.QuestionText,
        question.PhotoUrl,
        question.Points,
        question.OrderIndex,
        question.CreatedAt,
        question.UpdatedAt);
}