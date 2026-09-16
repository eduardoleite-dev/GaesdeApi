using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class Course
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("cover_image")]
    public string? CoverImage { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    [BsonElement("level")]
    [BsonRepresentation(BsonType.String)]
    public CourseLevel Level { get; set; }

    [BsonElement("instructor_id")]
    public string InstructorId { get; set; } = string.Empty;

    [BsonElement("category_id")]
    public string? CategoryId { get; set; }

    [BsonElement("published_at")]
    public DateTime? PublishedAt { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}