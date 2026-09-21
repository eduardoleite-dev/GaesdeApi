using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class QuizAttempt
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("quiz_id")]
    public string QuizId { get; set; } = string.Empty;

    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("enrollment_id")]
    public string EnrollmentId { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;

    [BsonElement("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    [BsonElement("total_score")]
    public decimal? TotalScore { get; set; }

    [BsonElement("is_passed")]
    public bool? IsPassed { get; set; }
}
