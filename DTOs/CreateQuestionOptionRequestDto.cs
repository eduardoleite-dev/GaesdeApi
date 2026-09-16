namespace GaesdeApi.DTOs;

public record CreateQuestionOptionRequestDto(
    string QuestionId,
    string OptionText,
    bool IsCorrect = false
);