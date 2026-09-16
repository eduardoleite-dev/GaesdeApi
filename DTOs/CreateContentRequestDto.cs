using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CreateContentRequestDto(
    string ModuleId,
    string Title,
    ContentType Type,
    int OrderIndex,
    bool IsFreePreview = false,
    int? DurationSeconds = null,
    string? VideoUrl = null,
    string? Body = null,
    string? FileUrl = null,
    long? FileSizeBytes = null
);