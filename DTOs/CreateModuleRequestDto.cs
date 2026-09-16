namespace GaesdeApi.DTOs;

public record CreateModuleRequestDto(
    string CourseId,
    string Title,
    int OrderIndex,
    string? Description = null
);