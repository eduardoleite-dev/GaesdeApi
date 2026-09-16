namespace GaesdeApi.DTOs;

public record CreateAssignmentSubmissionRequestDto(
    string ContentId,
    string EnrollmentId,
    string FileUrl
);