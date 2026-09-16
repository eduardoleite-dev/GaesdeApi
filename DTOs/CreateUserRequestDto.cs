using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CreateUserRequestDto(
    string Name,
    string Email,
    string Password,
    AccessLevel AccessLevel,
    string? AvatarUrl = null,
    string? Bio = null
);