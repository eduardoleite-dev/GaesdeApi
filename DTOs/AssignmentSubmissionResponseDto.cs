namespace GaesdeApi.DTOs;

public record AssignmentSubmissionResponseDto(
    string Id,
    string ContentId,
    string UserId,
    string EnrollmentId,
    string FileUrl,
    DateTime SubmittedAt,
    decimal? Grade,
    string? InstructorFeedback,
    DateTime? GradedAt,
    bool IsGraded
);