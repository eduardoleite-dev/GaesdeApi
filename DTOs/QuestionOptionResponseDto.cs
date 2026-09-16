namespace GaesdeApi.DTOs;

public record QuestionOptionResponseDto(
    string Id,
    string QuestionId,
    string OptionText,
    bool IsCorrect,
    DateTime CreatedAt
);