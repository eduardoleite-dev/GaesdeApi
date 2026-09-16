namespace GaesdeApi.DTOs;

public record UpdateQuestionOptionRequestDto(
    string OptionText,
    bool IsCorrect = false
);