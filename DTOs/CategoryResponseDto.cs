namespace GaesdeApi.DTOs;

public record CategoryResponseDto(
    string Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);