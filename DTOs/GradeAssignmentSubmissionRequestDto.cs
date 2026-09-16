namespace GaesdeApi.DTOs;

public record GradeAssignmentSubmissionRequestDto(
    decimal Grade,
    string? InstructorFeedback = null
);