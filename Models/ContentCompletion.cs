using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class ContentCompletion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("content_id")]
    public string ContentId { get; set; } = string.Empty;

    [BsonElement("completed_at")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
