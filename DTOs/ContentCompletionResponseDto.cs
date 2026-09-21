namespace GaesdeApi.DTOs;

public record ContentCompletionResponseDto(
    string Id,
    string UserId,
    string ContentId,
    DateTime CompletedAt
);
