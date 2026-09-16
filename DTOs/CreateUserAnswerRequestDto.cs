namespace GaesdeApi.DTOs;

public record CreateUserAnswerRequestDto(
    string AttemptId,
    string QuestionId,
    string? SelectedOptionId = null,
    IReadOnlyCollection<string>? SelectedOptionIds = null,
    string? TextResponse = null
);