using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class CommentAttachment
{
    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("public_id")]
    public string PublicId { get; set; } = string.Empty;

    [BsonElement("file_name")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("file_type")]
    public string FileType { get; set; } = string.Empty;

    [BsonElement("file_size")]
    public long FileSize { get; set; }
}

public class CommentReaction
{
    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("emoji")]
    public string Emoji { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public CommentType Type { get; set; }

    [BsonElement("content")]
    public string? Content { get; set; }

    [BsonElement("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [BsonElement("course_id")]
    public string? CourseId { get; set; }

    [BsonElement("recipient_ids")]
    public IReadOnlyCollection<string> RecipientIds { get; set; } = Array.Empty<string>();

    [BsonElement("parent_id")]
    public string? ParentId { get; set; }

    [BsonElement("attachments")]
    public IReadOnlyCollection<CommentAttachment> Attachments { get; set; } = Array.Empty<CommentAttachment>();

    [BsonElement("reactions")]
    public IReadOnlyCollection<CommentReaction> Reactions { get; set; } = Array.Empty<CommentReaction>();

    [BsonElement("archived_by")]
    public IReadOnlyCollection<string> ArchivedBy { get; set; } = Array.Empty<string>();

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}