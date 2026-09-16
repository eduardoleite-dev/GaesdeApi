using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class AssignmentSubmission
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("content_id")]
    public string ContentId { get; set; } = string.Empty;

    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("enrollment_id")]
    public string EnrollmentId { get; set; } = string.Empty;

    [BsonElement("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [BsonElement("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("grade")]
    public decimal? Grade { get; set; }

    [BsonElement("instructor_feedback")]
    public string? InstructorFeedback { get; set; }

    [BsonElement("graded_at")]
    public DateTime? GradedAt { get; set; }
}