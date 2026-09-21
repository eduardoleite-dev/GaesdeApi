using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record QuizAttemptResponseDto(
    string Id,
    string QuizId,
    string UserId,
    string EnrollmentId,
    QuizAttemptStatus Status,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    decimal? TotalScore,
    bool? IsPassed
);
