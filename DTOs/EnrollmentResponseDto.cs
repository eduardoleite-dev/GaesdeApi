using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record EnrollmentResponseDto(
    string Id,
    string UserId,
    string CourseId,
    string? PhotoUrl,
    EnrollmentStatus Status,
    decimal ProgressPercentage,
    DateTime EnrolledAt,
    DateTime? ExpiresAt,
    DateTime? LastAccessedAt,
    bool IsActive,
    bool IsCompleted,
    bool IsExpired
);