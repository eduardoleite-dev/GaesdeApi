using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class CommentService : ICommentService
{
    private readonly IMongoCollection<Comment> _commentsCollection;
    private readonly IMongoCollection<Course> _coursesCollection;

    public CommentService(IMongoDatabase database)
    {
        _commentsCollection = database.GetCollection<Comment>("Comments");
        _coursesCollection = database.GetCollection<Course>("Courses");
    }

    public async Task<IReadOnlyCollection<CommentResponseDto>> GetAllAsync(
        string userId,
        bool isAdministrator,
        string? courseId = null)
    {
        var filter = Builders<Comment>.Filter.Eq(comment => comment.DeletedAt, null);
        if (!isAdministrator)
            filter &= Builders<Comment>.Filter.Or(
                Builders<Comment>.Filter.Eq(comment => comment.AuthorId, userId),
                Builders<Comment>.Filter.AnyIn(comment => comment.RecipientIds, new[] { userId }));
        if (!string.IsNullOrWhiteSpace(courseId))
            filter &= Builders<Comment>.Filter.Eq(comment => comment.CourseId, courseId);

        var comments = await _commentsCollection
            .Find(filter)
            .SortByDescending(comment => comment.CreatedAt)
            .ToListAsync();

        return comments
            .Where(comment => isAdministrator || !comment.ArchivedBy.Contains(userId))
            .Select(ToResponse)
            .ToArray();
    }

    public async Task<CommentResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<Comment>.Filter.And(
            Builders<Comment>.Filter.Eq(comment => comment.Id, id),
            Builders<Comment>.Filter.Eq(comment => comment.DeletedAt, null));
        if (!isAdministrator)
            filter &= Builders<Comment>.Filter.Or(
                Builders<Comment>.Filter.Eq(comment => comment.AuthorId, userId),
                Builders<Comment>.Filter.AnyIn(comment => comment.RecipientIds, new[] { userId }));

        var comment = await _commentsCollection.Find(filter).FirstOrDefaultAsync();
        return comment is null || (!isAdministrator && comment.ArchivedBy.Contains(userId))
            ? null
            : ToResponse(comment);
    }

    public async Task<CommentResponseDto?> CreateAsync(string authorId, CreateCommentRequestDto request)
    {
        if (!IsValid(request.Type, request.Content, request.RecipientIds, request.CourseId, request.Attachments) ||
            !await CourseExistsAsync(request.Type, request.CourseId) ||
            !await ParentExistsAsync(request.ParentId))
            return null;

        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            Type = request.Type,
            Content = request.Content,
            AuthorId = authorId,
            CourseId = request.CourseId,
            RecipientIds = request.RecipientIds,
            ParentId = request.ParentId,
            Attachments = ToAttachments(request.Attachments),
            Reactions = Array.Empty<CommentReaction>(),
            ArchivedBy = Array.Empty<string>(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _commentsCollection.InsertOneAsync(comment);
        return ToResponse(comment);
    }

    public async Task<CommentResponseDto?> UpdateAsync(
        string id,
        string userId,
        bool isAdministrator,
        UpdateCommentRequestDto request)
    {
        if (!IsValidContent(request.Content, request.RecipientIds, request.Attachments))
            return null;

        var filter = isAdministrator
            ? Builders<Comment>.Filter.Eq(comment => comment.Id, id)
            : Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(comment => comment.Id, id),
                Builders<Comment>.Filter.Eq(comment => comment.AuthorId, userId));
        var comment = await _commentsCollection.Find(filter).FirstOrDefaultAsync();
        if (comment is null || comment.DeletedAt is not null)
            return null;

        comment.Content = request.Content;
        comment.RecipientIds = request.RecipientIds;
        comment.Attachments = ToAttachments(request.Attachments);
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentsCollection.ReplaceOneAsync(existingComment => existingComment.Id == id, comment);
        return ToResponse(comment);
    }

    public async Task<bool> DeleteAsync(string id, string userId, bool isAdministrator)
    {
        var filter = isAdministrator
            ? Builders<Comment>.Filter.Eq(comment => comment.Id, id)
            : Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(comment => comment.Id, id),
                Builders<Comment>.Filter.Eq(comment => comment.AuthorId, userId));
        var update = Builders<Comment>.Update
            .Set(comment => comment.DeletedAt, DateTime.UtcNow)
            .Set(comment => comment.UpdatedAt, DateTime.UtcNow);
        return (await _commentsCollection.UpdateOneAsync(filter, update)).ModifiedCount > 0;
    }

    public async Task<CommentResponseDto?> AddReactionAsync(string id, string userId, CommentReactionRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Emoji))
            return null;

        var comment = await FindVisibleAsync(id, userId, true);
        if (comment is null)
            return null;

        var reactions = comment.Reactions.Where(reaction => reaction.UserId != userId).ToList();
        reactions.Add(new CommentReaction { UserId = userId, Emoji = request.Emoji.Trim(), CreatedAt = DateTime.UtcNow });
        comment.Reactions = reactions;
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentsCollection.ReplaceOneAsync(existingComment => existingComment.Id == id, comment);
        return ToResponse(comment);
    }

    public Task<CommentResponseDto?> RemoveReactionAsync(string id, string userId) =>
        ChangeReactionAsync(id, userId, reactions => reactions.Where(reaction => reaction.UserId != userId).ToList());

    public Task<CommentResponseDto?> ArchiveAsync(string id, string userId) =>
        ChangeArchiveAsync(id, userId, archivedBy => archivedBy.Append(userId).Distinct().ToArray());

    public Task<CommentResponseDto?> UnarchiveAsync(string id, string userId) =>
        ChangeArchiveAsync(id, userId, archivedBy => archivedBy.Where(value => value != userId).ToArray());

    private async Task<CommentResponseDto?> ChangeReactionAsync(
        string id,
        string userId,
        Func<IEnumerable<CommentReaction>, IReadOnlyCollection<CommentReaction>> change)
    {
        var comment = await FindVisibleAsync(id, userId, true);
        if (comment is null)
            return null;
        comment.Reactions = change(comment.Reactions);
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentsCollection.ReplaceOneAsync(existingComment => existingComment.Id == id, comment);
        return ToResponse(comment);
    }

    private async Task<CommentResponseDto?> ChangeArchiveAsync(
        string id,
        string userId,
        Func<IEnumerable<string>, IReadOnlyCollection<string>> change)
    {
        var comment = await FindVisibleAsync(id, userId, false);
        if (comment is null)
            return null;
        comment.ArchivedBy = change(comment.ArchivedBy);
        comment.UpdatedAt = DateTime.UtcNow;
        await _commentsCollection.ReplaceOneAsync(existingComment => existingComment.Id == id, comment);
        return ToResponse(comment);
    }

    private async Task<Comment?> FindVisibleAsync(string id, string userId, bool includeArchived)
    {
        var comment = await _commentsCollection.Find(existingComment =>
                existingComment.Id == id && existingComment.DeletedAt == null)
            .FirstOrDefaultAsync();
        return comment is null || (!includeArchived && comment.ArchivedBy.Contains(userId)) ? null : comment;
    }

    private async Task<bool> CourseExistsAsync(CommentType type, string? courseId)
    {
        return type == CommentType.Chat || !string.IsNullOrWhiteSpace(courseId) && await _coursesCollection
            .Find(course => course.Id == courseId && course.DeletedAt == null)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> ParentExistsAsync(string? parentId)
    {
        return string.IsNullOrWhiteSpace(parentId) || await _commentsCollection
            .Find(comment => comment.Id == parentId && comment.DeletedAt == null)
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(CommentType type, string? content, IReadOnlyCollection<string> recipients, string? courseId, IReadOnlyCollection<CommentAttachmentDto>? attachments) =>
        Enum.IsDefined(type) && IsValidContent(content, recipients, attachments) &&
        (type == CommentType.Chat || !string.IsNullOrWhiteSpace(courseId));

    private static bool IsValidContent(string? content, IReadOnlyCollection<string> recipients, IReadOnlyCollection<CommentAttachmentDto>? attachments) =>
        recipients.Count > 0 && (content?.Length ?? 0) <= 5000 &&
        (!string.IsNullOrWhiteSpace(content) || attachments?.Count > 0) &&
        (attachments is null || attachments.All(attachment => attachment.FileSize >= 0 &&
            !string.IsNullOrWhiteSpace(attachment.Url) && !string.IsNullOrWhiteSpace(attachment.FileName)));

    private static IReadOnlyCollection<CommentAttachment> ToAttachments(IReadOnlyCollection<CommentAttachmentDto>? attachments) =>
        attachments?.Select(attachment => new CommentAttachment
        {
            Url = attachment.Url,
            PublicId = attachment.PublicId,
            FileName = attachment.FileName,
            FileType = attachment.FileType,
            FileSize = attachment.FileSize
        }).ToArray() ?? Array.Empty<CommentAttachment>();

    private static CommentResponseDto ToResponse(Comment comment) => new(
        comment.Id,
        comment.Type,
        comment.Content,
        comment.AuthorId,
        comment.CourseId,
        comment.RecipientIds,
        comment.ParentId,
        comment.Attachments.Select(attachment => new CommentAttachmentDto(
            attachment.Url, attachment.PublicId, attachment.FileName, attachment.FileType, attachment.FileSize)).ToArray(),
        comment.Reactions,
        comment.ArchivedBy,
        comment.CreatedAt,
        comment.UpdatedAt,
        comment.DeletedAt);
}