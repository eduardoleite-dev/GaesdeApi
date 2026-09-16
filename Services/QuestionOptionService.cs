using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class QuestionOptionService : IQuestionOptionService
{
    private readonly IMongoCollection<QuestionOption> _optionsCollection;
    private readonly IMongoCollection<Question> _questionsCollection;

    public QuestionOptionService(IMongoDatabase database)
    {
        _optionsCollection = database.GetCollection<QuestionOption>("QuestionOptions");
        _questionsCollection = database.GetCollection<Question>("Questions");
    }

    public async Task<IReadOnlyCollection<QuestionOptionResponseDto>> GetAllAsync(string? questionId = null)
    {
        var filter = string.IsNullOrWhiteSpace(questionId)
            ? Builders<QuestionOption>.Filter.Empty
            : Builders<QuestionOption>.Filter.Eq(option => option.QuestionId, questionId);

        var options = await _optionsCollection
            .Find(filter)
            .SortBy(option => option.CreatedAt)
            .ToListAsync();

        return options.Select(ToResponse).ToArray();
    }

    public async Task<QuestionOptionResponseDto?> GetByIdAsync(string id)
    {
        var option = await _optionsCollection
            .Find(existingOption => existingOption.Id == id)
            .FirstOrDefaultAsync();

        return option is null ? null : ToResponse(option);
    }

    public async Task<QuestionOptionResponseDto?> CreateAsync(CreateQuestionOptionRequestDto request)
    {
        if (!IsValid(request.QuestionId, request.OptionText) ||
            !await QuestionSupportsOptionsAsync(request.QuestionId) ||
            await OptionTextExistsAsync(request.QuestionId, request.OptionText))
            return null;

        var option = new QuestionOption
        {
            QuestionId = request.QuestionId.Trim(),
            OptionText = request.OptionText.Trim(),
            IsCorrect = request.IsCorrect,
            CreatedAt = DateTime.UtcNow
        };

        await _optionsCollection.InsertOneAsync(option);
        return ToResponse(option);
    }

    public async Task<QuestionOptionResponseDto?> UpdateAsync(string id, UpdateQuestionOptionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.OptionText))
            return null;

        var option = await _optionsCollection
            .Find(existingOption => existingOption.Id == id)
            .FirstOrDefaultAsync();

        if (option is null || await OptionTextExistsAsync(option.QuestionId, request.OptionText, id))
            return null;

        option.OptionText = request.OptionText.Trim();
        option.IsCorrect = request.IsCorrect;

        await _optionsCollection.ReplaceOneAsync(existingOption => existingOption.Id == id, option);
        return ToResponse(option);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _optionsCollection.DeleteOneAsync(option => option.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> QuestionSupportsOptionsAsync(string questionId)
    {
        return await _questionsCollection.Find(question =>
                question.Id == questionId &&
                (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse))
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> OptionTextExistsAsync(string questionId, string optionText, string? excludedId = null)
    {
        return await _optionsCollection.Find(option =>
                option.QuestionId == questionId &&
                option.OptionText == optionText.Trim() &&
                (excludedId == null || option.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(string questionId, string optionText) =>
        !string.IsNullOrWhiteSpace(questionId) && !string.IsNullOrWhiteSpace(optionText);

    private static QuestionOptionResponseDto ToResponse(QuestionOption option) => new(
        option.Id,
        option.QuestionId,
        option.OptionText,
        option.IsCorrect,
        option.CreatedAt);
}