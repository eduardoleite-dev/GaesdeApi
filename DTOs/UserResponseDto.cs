using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record UserResponseDto(
    string Id,
    string Name,
    string Email,
    string? AvatarUrl,
    string? Bio,
    DateTime? EmailVerifiedAt,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    AccessLevel AccessLevel
);