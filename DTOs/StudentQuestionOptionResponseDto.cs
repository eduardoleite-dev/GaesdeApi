namespace GaesdeApi.DTOs;

public record StudentQuestionOptionResponseDto(
    string Id,
    string QuestionId,
    string OptionText,
    DateTime CreatedAt
);
