using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class Question
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("quiz_id")]
    public string QuizId { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public QuestionType Type { get; set; }

    [BsonElement("question_text")]
    public string QuestionText { get; set; } = string.Empty;

    [BsonElement("photo_url")]
    public string? PhotoUrl { get; set; }

    [BsonElement("points")]
    public decimal Points { get; set; } = 1;

    [BsonElement("order_index")]
    public int OrderIndex { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}