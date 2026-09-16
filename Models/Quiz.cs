using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class Quiz
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("content_id")]
    public string ContentId { get; set; } = string.Empty;

    [BsonElement("time_limit_minutes")]
    public int? TimeLimitMinutes { get; set; }

    [BsonElement("passing_score_percentage")]
    public decimal PassingScorePercentage { get; set; } = 60;

    [BsonElement("attempts_allowed")]
    public int AttemptsAllowed { get; set; } = 1;

    [BsonElement("shuffle_questions")]
    public bool ShuffleQuestions { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}