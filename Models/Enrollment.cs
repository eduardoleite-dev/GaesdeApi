using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class Enrollment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("course_id")]
    public string CourseId { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    [BsonElement("progress_percentage")]
    public decimal ProgressPercentage { get; set; }

    [BsonElement("enrolled_at")]
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("last_accessed_at")]
    public DateTime? LastAccessedAt { get; set; }
}