namespace GaesdeApi.DTOs;

public record UpdateCommentRequestDto(
    string? Content,
    IReadOnlyCollection<string> RecipientIds,
    IReadOnlyCollection<CommentAttachmentDto>? Attachments = null
);