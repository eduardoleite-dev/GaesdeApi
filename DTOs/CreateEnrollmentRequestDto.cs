namespace GaesdeApi.DTOs;

public record CreateEnrollmentRequestDto(
    string CourseId,
    string? UserId = null,
    DateTime? ExpiresAt = null
);