using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record ContentResponseDto(
    string Id,
    string ModuleId,
    string Title,
    ContentType Type,
    int OrderIndex,
    bool IsFreePreview,
    int? DurationSeconds,
    string? VideoUrl,
    string? Body,
    string? FileUrl,
    long? FileSizeBytes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);