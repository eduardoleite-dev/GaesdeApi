namespace GaesdeApi.DTOs;

public record StartQuizAttemptRequestDto(
    string QuizId,
    string EnrollmentId
);
