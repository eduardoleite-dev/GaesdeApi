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
        private readonly IMongoCollection<QuizAttempt> _attemptsCollection;

    public UserAnswerService(IMongoDatabase database)
    {
        _answersCollection = database.GetCollection<UserAnswer>("UserAnswers");
        _questionsCollection = database.GetCollection<Question>("Questions");
        _optionsCollection = database.GetCollection<QuestionOption>("QuestionOptions");
            _attemptsCollection = database.GetCollection<QuizAttempt>("QuizAttempts");
    }

        public async Task<IReadOnlyCollection<UserAnswerResponseDto>> GetAllAsync(
            string? attemptId = null,
            string? userId = null,
            bool isAdministrator = false)
    {
        var filter = string.IsNullOrWhiteSpace(attemptId)
            ? Builders<UserAnswer>.Filter.Empty
            : Builders<UserAnswer>.Filter.Eq(answer => answer.AttemptId, attemptId);

            if (!isAdministrator && !string.IsNullOrWhiteSpace(userId))
            {
                var attemptIds = await _attemptsCollection.Find(value => value.UserId == userId)
                    .Project(value => value.Id)
                    .ToListAsync();
                filter &= Builders<UserAnswer>.Filter.In(answer => answer.AttemptId, attemptIds);
            }

        var answers = await _answersCollection
            .Find(filter)
            .SortBy(answer => answer.CreatedAt)
            .ToListAsync();

        return answers.Select(ToResponse).ToArray();
    }

    public async Task<UserAnswerResponseDto?> GetByIdAsync(string id, string? userId = null, bool isAdministrator = false)
    {
        var filter = Builders<UserAnswer>.Filter.Eq(answer => answer.Id, id);
        if (!isAdministrator && !string.IsNullOrWhiteSpace(userId))
            filter &= await OwnedAnswerFilterAsync(userId);
        var answer = await _answersCollection.Find(filter)
            .FirstOrDefaultAsync();

        return answer is null ? null : ToResponse(answer);
    }

    public async Task<UserAnswerResponseDto?> CreateAsync(CreateUserAnswerRequestDto request, string? userId = null)
    {
        if (!IsValid(request.AttemptId, request.QuestionId, request.SelectedOptionId,
            request.SelectedOptionIds, request.TextResponse))
            return null;

        var attempt = await _attemptsCollection.Find(value =>
                value.Id == request.AttemptId &&
                (userId == null || value.UserId == userId) &&
                value.Status == QuizAttemptStatus.InProgress)
            .FirstOrDefaultAsync();
        var question = await _questionsCollection.Find(value => value.Id == request.QuestionId).FirstOrDefaultAsync();
        if (attempt is null || question is null ||
            question.QuizId != attempt.QuizId ||
            !await OptionsBelongToQuestionAsync(request.QuestionId, request.SelectedOptionId, request.SelectedOptionIds) ||
            await AnswerExistsAsync(request.AttemptId, request.QuestionId))
            return null;

        var grading = await GradeAsync(question, request.SelectedOptionId, request.SelectedOptionIds);

        var answer = new UserAnswer
        {
            AttemptId = request.AttemptId.Trim(),
            QuestionId = request.QuestionId.Trim(),
            SelectedOptionId = request.SelectedOptionId,
            SelectedOptionIds = request.SelectedOptionIds,
            TextResponse = request.TextResponse,
            IsCorrect = grading.IsCorrect,
            PointsEarned = grading.PointsEarned,
            CreatedAt = DateTime.UtcNow
        };

        await _answersCollection.InsertOneAsync(answer);
        return ToResponse(answer);
    }

    public async Task<UserAnswerResponseDto?> UpdateAsync(
        string id,
        UpdateUserAnswerRequestDto request,
        string? userId = null,
        bool isAdministrator = false)
    {
        if (request.PointsEarned < 0 ||
            !IsValidSelection(request.SelectedOptionId, request.SelectedOptionIds, request.TextResponse))
            return null;

        var filter = Builders<UserAnswer>.Filter.Eq(answer => answer.Id, id);
        if (!isAdministrator && !string.IsNullOrWhiteSpace(userId))
            filter &= await OwnedAnswerFilterAsync(userId);
        var answer = await _answersCollection.Find(filter)
            .FirstOrDefaultAsync();

        if (answer is null ||
            !await OptionsBelongToQuestionAsync(answer.QuestionId, request.SelectedOptionId, request.SelectedOptionIds))
            return null;

        var attempt = await _attemptsCollection.Find(value => value.Id == answer.AttemptId)
            .FirstOrDefaultAsync();
        if (attempt is null || attempt.Status != QuizAttemptStatus.InProgress)
            return null;

        answer.SelectedOptionId = request.SelectedOptionId;
        answer.SelectedOptionIds = request.SelectedOptionIds;
        answer.TextResponse = request.TextResponse;
            var question = await _questionsCollection.Find(value => value.Id == answer.QuestionId).FirstOrDefaultAsync();
            if (question is not null)
            {
                var grading = await GradeAsync(question, request.SelectedOptionId, request.SelectedOptionIds);
                answer.IsCorrect = grading.IsCorrect;
                answer.PointsEarned = grading.PointsEarned;
            }

        await _answersCollection.ReplaceOneAsync(existingAnswer => existingAnswer.Id == id, answer);
        return ToResponse(answer);
    }

    public async Task<bool> DeleteAsync(string id, string? userId = null, bool isAdministrator = false)
    {
        var filter = Builders<UserAnswer>.Filter.Eq(answer => answer.Id, id);
        if (!isAdministrator && !string.IsNullOrWhiteSpace(userId))
            filter &= await OwnedAnswerFilterAsync(userId);
        var result = await _answersCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    private async Task<FilterDefinition<UserAnswer>> OwnedAnswerFilterAsync(string userId)
    {
        var attemptIds = await _attemptsCollection.Find(value => value.UserId == userId)
            .Project(value => value.Id)
            .ToListAsync();
        return Builders<UserAnswer>.Filter.In(answer => answer.AttemptId, attemptIds);
    }

    private async Task<(bool? IsCorrect, decimal PointsEarned)> GradeAsync(
        Question question,
        string? selectedOptionId,
        IReadOnlyCollection<string>? selectedOptionIds)
    {
        var selectedIds = new[] { selectedOptionId }
            .Concat(selectedOptionIds ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToHashSet();
        if (selectedIds.Count == 0 || question.Type == QuestionType.Essay)
            return (null, 0);

        var correctIds = (await _optionsCollection.Find(value =>
                value.QuestionId == question.Id && value.IsCorrect)
            .Project(value => value.Id)
            .ToListAsync())
            .ToHashSet();
        var isCorrect = selectedIds.SetEquals(correctIds);
        return (isCorrect, isCorrect ? question.Points : 0);
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