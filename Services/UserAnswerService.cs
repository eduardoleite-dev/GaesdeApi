using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class UserAnswerService : IUserAnswerService
{
    private readonly IMongoCollection<UserAnswer> _answersCollection;
    private readonly IMongoCollection<Question> _questionsCollection;
    private readonly IMongoCollection<QuestionOption> _optionsCollection;

    public UserAnswerService(IMongoDatabase database)
    {
        _answersCollection = database.GetCollection<UserAnswer>("UserAnswers");
        _questionsCollection = database.GetCollection<Question>("Questions");
        _optionsCollection = database.GetCollection<QuestionOption>("QuestionOptions");
    }

    public async Task<IReadOnlyCollection<UserAnswerResponseDto>> GetAllAsync(string? attemptId = null)
    {
        var filter = string.IsNullOrWhiteSpace(attemptId)
            ? Builders<UserAnswer>.Filter.Empty
            : Builders<UserAnswer>.Filter.Eq(answer => answer.AttemptId, attemptId);

        var answers = await _answersCollection
            .Find(filter)
            .SortBy(answer => answer.CreatedAt)
            .ToListAsync();

        return answers.Select(ToResponse).ToArray();
    }

    public async Task<UserAnswerResponseDto?> GetByIdAsync(string id)
    {
        var answer = await _answersCollection
            .Find(existingAnswer => existingAnswer.Id == id)
            .FirstOrDefaultAsync();

        return answer is null ? null : ToResponse(answer);
    }

    public async Task<UserAnswerResponseDto?> CreateAsync(CreateUserAnswerRequestDto request)
    {
        if (!IsValid(request.AttemptId, request.QuestionId, request.SelectedOptionId,
                request.SelectedOptionIds, request.TextResponse) ||
            !await QuestionExistsAsync(request.QuestionId) ||
            !await OptionsBelongToQuestionAsync(request.QuestionId, request.SelectedOptionId, request.SelectedOptionIds) ||
            await AnswerExistsAsync(request.AttemptId, request.QuestionId))
            return null;

        var answer = new UserAnswer
        {
            AttemptId = request.AttemptId.Trim(),
            QuestionId = request.QuestionId.Trim(),
            SelectedOptionId = request.SelectedOptionId,
            SelectedOptionIds = request.SelectedOptionIds,
            TextResponse = request.TextResponse,
            CreatedAt = DateTime.UtcNow
        };

        await _answersCollection.InsertOneAsync(answer);
        return ToResponse(answer);
    }

    public async Task<UserAnswerResponseDto?> UpdateAsync(string id, UpdateUserAnswerRequestDto request)
    {
        if (request.PointsEarned < 0 ||
            !IsValidSelection(request.SelectedOptionId, request.SelectedOptionIds, request.TextResponse))
            return null;

        var answer = await _answersCollection
            .Find(existingAnswer => existingAnswer.Id == id)
            .FirstOrDefaultAsync();

        if (answer is null ||
            !await OptionsBelongToQuestionAsync(answer.QuestionId, request.SelectedOptionId, request.SelectedOptionIds))
            return null;

        answer.SelectedOptionId = request.SelectedOptionId;
        answer.SelectedOptionIds = request.SelectedOptionIds;
        answer.TextResponse = request.TextResponse;
        answer.IsCorrect = request.IsCorrect;
        answer.PointsEarned = request.PointsEarned;

        await _answersCollection.ReplaceOneAsync(existingAnswer => existingAnswer.Id == id, answer);
        return ToResponse(answer);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _answersCollection.DeleteOneAsync(answer => answer.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> QuestionExistsAsync(string questionId)
    {
        return await _questionsCollection.Find(question => question.Id == questionId)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> OptionsBelongToQuestionAsync(
        string questionId,
        string? selectedOptionId,
        IReadOnlyCollection<string>? selectedOptionIds)
    {
        var optionIds = new[] { selectedOptionId }
            .Concat(selectedOptionIds ?? Array.Empty<string>())
            .Where(optionId => !string.IsNullOrWhiteSpace(optionId))
            .Distinct()
            .ToArray();

        if (optionIds.Length == 0)
            return true;

        var count = await _optionsCollection.Find(option =>
                option.QuestionId == questionId && optionIds.Contains(option.Id))
            .CountDocumentsAsync();

        return count == optionIds.Length;
    }

    private async Task<bool> AnswerExistsAsync(string attemptId, string questionId)
    {
        return await _answersCollection.Find(answer =>
                answer.AttemptId == attemptId && answer.QuestionId == questionId)
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(
        string attemptId,
        string questionId,
        string? selectedOptionId,
        IReadOnlyCollection<string>? selectedOptionIds,
        string? textResponse)
    {
        return !string.IsNullOrWhiteSpace(attemptId) &&
            !string.IsNullOrWhiteSpace(questionId) &&
            IsValidSelection(selectedOptionId, selectedOptionIds, textResponse);
    }

    private static bool IsValidSelection(
        string? selectedOptionId,
        IReadOnlyCollection<string>? selectedOptionIds,
        string? textResponse)
    {
        return (selectedOptionId is null || !string.IsNullOrWhiteSpace(selectedOptionId)) &&
            (selectedOptionIds is null || selectedOptionIds.All(optionId => !string.IsNullOrWhiteSpace(optionId))) &&
            (textResponse is null || !string.IsNullOrWhiteSpace(textResponse));
    }

    private static UserAnswerResponseDto ToResponse(UserAnswer answer) => new(
        answer.Id,
        answer.AttemptId,
        answer.QuestionId,
        answer.SelectedOptionId,
        answer.SelectedOptionIds,
        answer.TextResponse,
        answer.IsCorrect,
        answer.PointsEarned,
        answer.CreatedAt);
}