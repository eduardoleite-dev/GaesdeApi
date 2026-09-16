using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class UserAnswer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("attempt_id")]
    public string AttemptId { get; set; } = string.Empty;

    [BsonElement("question_id")]
    public string QuestionId { get; set; } = string.Empty;

    [BsonElement("selected_option_id")]
    public string? SelectedOptionId { get; set; }

    [BsonElement("selected_option_ids")]
    public IReadOnlyCollection<string>? SelectedOptionIds { get; set; }

    [BsonElement("text_response")]
    public string? TextResponse { get; set; }

    [BsonElement("is_correct")]
    public bool? IsCorrect { get; set; }

    [BsonElement("points_earned")]
    public decimal PointsEarned { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}