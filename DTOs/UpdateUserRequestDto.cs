using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record UpdateUserRequestDto(
    string Name,
    string Email,
    AccessLevel AccessLevel,
    string? Password = null,
    string? AvatarUrl = null,
    string? Bio = null
);