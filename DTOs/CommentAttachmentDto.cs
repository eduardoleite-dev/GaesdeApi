namespace GaesdeApi.DTOs;

public record CommentAttachmentDto(
    string Url,
    string PublicId,
    string FileName,
    string FileType,
    long FileSize
);