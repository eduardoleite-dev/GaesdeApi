using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class Content
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("module_id")]
    public string ModuleId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public ContentType Type { get; set; }

    [BsonElement("order_index")]
    public int OrderIndex { get; set; }

    [BsonElement("is_free_preview")]
    public bool IsFreePreview { get; set; }

    [BsonElement("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [BsonElement("video_url")]
    public string? VideoUrl { get; set; }

    [BsonElement("body")]
    public string? Body { get; set; }

    [BsonElement("file_url")]
    public string? FileUrl { get; set; }

    [BsonElement("file_size_bytes")]
    public long? FileSizeBytes { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}