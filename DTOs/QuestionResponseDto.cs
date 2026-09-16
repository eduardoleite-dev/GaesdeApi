using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record QuestionResponseDto(
    string Id,
    string QuizId,
    QuestionType Type,
    string QuestionText,
    decimal Points,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt
);