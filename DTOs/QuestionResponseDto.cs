using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record QuestionResponseDto(
    string Id,
    string QuizId,
    QuestionType Type,
    string QuestionText,
    string? PhotoUrl,
    decimal Points,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt
);