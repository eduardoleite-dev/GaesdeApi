using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CreateCommentRequestDto(
    CommentType Type,
    string? Content,
    IReadOnlyCollection<string> RecipientIds,
    string? CourseId = null,
    string? ParentId = null,
    IReadOnlyCollection<CommentAttachmentDto>? Attachments = null
);