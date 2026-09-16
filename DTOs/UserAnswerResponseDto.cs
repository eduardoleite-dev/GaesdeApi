namespace GaesdeApi.DTOs;

public record UserAnswerResponseDto(
    string Id,
    string AttemptId,
    string QuestionId,
    string? SelectedOptionId,
    IReadOnlyCollection<string>? SelectedOptionIds,
    string? TextResponse,
    bool? IsCorrect,
    decimal PointsEarned,
    DateTime CreatedAt
);