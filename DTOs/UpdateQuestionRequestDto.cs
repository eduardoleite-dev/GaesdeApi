using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record UpdateQuestionRequestDto(
    QuestionType Type,
    string QuestionText,
    decimal Points,
    int OrderIndex
);