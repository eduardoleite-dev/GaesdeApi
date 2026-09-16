namespace GaesdeApi.DTOs;

public record UpdateModuleRequestDto(
    string Title,
    int OrderIndex,
    string? Description = null
);