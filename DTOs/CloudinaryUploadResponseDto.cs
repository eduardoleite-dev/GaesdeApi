namespace GaesdeApi.DTOs;

public record CloudinaryUploadResponseDto(
    string Url,
    string PublicId,
    string ResourceType,
    string FileName,
    string ContentType,
    long FileSize
);