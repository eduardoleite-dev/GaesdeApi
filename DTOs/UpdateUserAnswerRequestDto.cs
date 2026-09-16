namespace GaesdeApi.DTOs;

public record UpdateUserAnswerRequestDto(
    string? SelectedOptionId = null,
    IReadOnlyCollection<string>? SelectedOptionIds = null,
    string? TextResponse = null,
    bool? IsCorrect = null,
    decimal PointsEarned = 0
);