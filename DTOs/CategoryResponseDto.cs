namespace GaesdeApi.DTOs;

public record CategoryResponseDto(
    string Id,
    string Name,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);