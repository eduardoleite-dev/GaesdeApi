namespace GaesdeApi.DTOs;

public record UpdateQuizRequestDto(
    decimal PassingScorePercentage,
    int AttemptsAllowed,
    bool ShuffleQuestions,
    int? TimeLimitMinutes = null
);