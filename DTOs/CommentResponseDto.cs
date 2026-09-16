using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CommentResponseDto(
    string Id,
    CommentType Type,
    string? Content,
    string AuthorId,
    string? CourseId,
    IReadOnlyCollection<string> RecipientIds,
    string? ParentId,
    IReadOnlyCollection<CommentAttachmentDto> Attachments,
    IReadOnlyCollection<CommentReaction> Reactions,
    IReadOnlyCollection<string> ArchivedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? DeletedAt
);