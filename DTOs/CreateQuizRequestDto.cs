namespace GaesdeApi.DTOs;

public record CreateQuizRequestDto(
    string ContentId,
    decimal PassingScorePercentage = 60,
    int AttemptsAllowed = 1,
    bool ShuffleQuestions = false,
    int? TimeLimitMinutes = null
);