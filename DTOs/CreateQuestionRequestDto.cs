using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CreateQuestionRequestDto(
    string QuizId,
    QuestionType Type,
    string QuestionText,
    decimal Points = 1,
    int OrderIndex = 0
);