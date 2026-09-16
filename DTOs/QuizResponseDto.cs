namespace GaesdeApi.DTOs;

public record QuizResponseDto(
    string Id,
    string ContentId,
    int? TimeLimitMinutes,
    decimal PassingScorePercentage,
    int AttemptsAllowed,
    bool ShuffleQuestions,
    DateTime CreatedAt,
    DateTime UpdatedAt
);