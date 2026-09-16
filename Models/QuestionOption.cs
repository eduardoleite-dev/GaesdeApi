using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class QuestionOption
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("question_id")]
    public string QuestionId { get; set; } = string.Empty;

    [BsonElement("option_text")]
    public string OptionText { get; set; } = string.Empty;

    [BsonElement("is_correct")]
    public bool IsCorrect { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}