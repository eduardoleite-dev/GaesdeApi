namespace GaesdeApi.DTOs;

public record ModuleResponseDto(
    string Id,
    string CourseId,
    string Title,
    string? Description,
    int OrderIndex,
    DateTime CreatedAt,
    DateTime UpdatedAt
);