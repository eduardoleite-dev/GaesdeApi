using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class CommentService : ICommentService
{
    private readonly IMongoCollection<Comment> _commentsCollection;
    private readonly IMongoCollection<Course> _coursesCollection;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<Enrollment> _enrollmentsCollection;

    public CommentService(IMongoDatabase database)
    {
        _commentsCollection = database.GetCollection<Comment>("Comments");
        _coursesCollection = database.GetCollection<Course>("Courses");
        _usersCollection = database.GetCollection<User>("Users");
        _enrollmentsCollection = database.GetCollection<Enrollment>("Enrollments");
    }

    public async Task<IReadOnlyCollection<CommentResponseDto>> GetAllAsync(
        string userId,
        AccessLevel accessLevel,
        string? courseId = null)
    {
        var filter = Builders<Comment>.Filter.Eq(comment => comment.DeletedAt, null);
        if (!string.IsNullOrWhiteSpace(courseId))
            filter &= Builders<Comment>.Filter.Eq(comment => comment.CourseId, courseId);

        var comments = await _commentsCollection
            .Find(filter)
            .SortByDescending(comment => comment.CreatedAt)
            .ToListAsync();

        var visibleComments = new List<Comment>();
        foreach (var comment in comments)
        {
            if (await CanViewAsync(comment, userId, accessLevel))
                visibleComments.Add(comment);
        }

        return visibleComments
            .Where(comment => accessLevel == AccessLevel.Administrador || !comment.ArchivedBy.Contains(userId))
            .Select(ToResponse)
            .ToArray();
    }

    public async Task<CommentResponseDto?> GetByIdAsync(string id, string userId, AccessLevel accessLevel)
    {
        var filter = Builders<Comment>.Filter.And(
            Builders<Comment>.Filter.Eq(comment => comment.Id, id),
            Builders<Comment>.Filter.Eq(comment => comment.DeletedAt, null));
        var comment = await _commentsCollection.Find(filter).FirstOrDefaultAsync();
        return comment is null || (!await CanViewAsync(comment, userId, accessLevel) ||
            accessLevel != AccessLevel.Administrador && comment.ArchivedBy.Contains(userId))
            ? null
            : ToResponse(comment);
    }

    public async Task<CommentResponseDto?> CreateAsync(
        string authorId,
        AccessLevel authorAccessLevel,
        CreateCommentRequestDto request)
    {
        if (!IsValid(request.Type, request.Content, request.RecipientIds, request.CourseId, request.Attachments) ||
            !await CourseExistsAsync(request.Type, request.CourseId) ||
            !await ParentExistsAsync(request.ParentId) ||
            !await CanCreateAsync(authorId, authorAccessLevel, request))
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
        if (comment is null || comment.DeletedAt is not null ||
            comment.Type == CommentType.Chat && request.RecipientIds.Count == 0)
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
        if (comment is null || (!includeArchived && comment.ArchivedBy.Contains(userId)))
            return null;

        var canSeeComment = comment.AuthorId == userId || comment.RecipientIds.Contains(userId);
        return canSeeComment ? comment : null;
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
        (type != CommentType.Chat || recipients.Count > 0) &&
        (type == CommentType.Chat || !string.IsNullOrWhiteSpace(courseId));

    private static bool IsValidContent(string? content, IReadOnlyCollection<string> recipients, IReadOnlyCollection<CommentAttachmentDto>? attachments) =>
        (content?.Length ?? 0) <= 5000 &&
        (!string.IsNullOrWhiteSpace(content) || attachments?.Count > 0) &&
        (attachments is null || attachments.All(attachment => attachment.FileSize >= 0 &&
            !string.IsNullOrWhiteSpace(attachment.Url) && !string.IsNullOrWhiteSpace(attachment.FileName)));

    private async Task<bool> CanCreateAsync(
        string authorId,
        AccessLevel accessLevel,
        CreateCommentRequestDto request)
    {
        if (request.Type == CommentType.Chat)
            return await RecipientsExistAsync(request.RecipientIds);

        if (request.CourseId is null)
            return false;

        if (accessLevel == AccessLevel.Administrador)
            return true;

        if (accessLevel == AccessLevel.Vendedor)
            return await IsPublishedCourseAsync(request.CourseId);

        if (accessLevel == AccessLevel.Professor)
            return await _coursesCollection.Find(course =>
                    course.Id == request.CourseId && course.InstructorId == authorId && course.DeletedAt == null)
                .Limit(1)
                .AnyAsync();

        return await _enrollmentsCollection.Find(enrollment =>
                enrollment.UserId == authorId && enrollment.CourseId == request.CourseId &&
                (enrollment.Status == EnrollmentStatus.Active || enrollment.Status == EnrollmentStatus.Completed) &&
                (enrollment.ExpiresAt == null || enrollment.ExpiresAt > DateTime.UtcNow))
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> CanViewAsync(Comment comment, string userId, AccessLevel accessLevel)
    {
        if (accessLevel == AccessLevel.Administrador)
            return true;

        if (comment.Type == CommentType.Chat)
            return comment.AuthorId == userId || comment.RecipientIds.Contains(userId);

        if (comment.CourseId is null)
            return false;

        if (accessLevel == AccessLevel.Vendedor)
            return await IsPublishedCourseAsync(comment.CourseId);

        if (accessLevel == AccessLevel.Professor)
            return await _coursesCollection.Find(course =>
                    course.Id == comment.CourseId && course.InstructorId == userId && course.DeletedAt == null)
                .Limit(1)
                .AnyAsync();

        return await _enrollmentsCollection.Find(enrollment =>
                enrollment.UserId == userId && enrollment.CourseId == comment.CourseId &&
                (enrollment.Status == EnrollmentStatus.Active || enrollment.Status == EnrollmentStatus.Completed) &&
                (enrollment.ExpiresAt == null || enrollment.ExpiresAt > DateTime.UtcNow))
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> IsPublishedCourseAsync(string? courseId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            return false;

        return await _coursesCollection.Find(course =>
                course.Id == courseId &&
                course.Status == CourseStatus.Published &&
                course.DeletedAt == null)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> RecipientsExistAsync(IReadOnlyCollection<string> recipientIds)
    {
        if (recipientIds.Count == 0)
            return false;
        var count = await _usersCollection.Find(user =>
                recipientIds.Contains(user.Id) && user.DeletedAt == null)
            .CountDocumentsAsync();
        return count == recipientIds.Distinct().Count();
    }

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