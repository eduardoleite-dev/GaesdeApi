using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GaesdeApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("avatar_url")]
    public string? AvatarUrl { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("email_verified_at")]
    public DateTime? EmailVerifiedAt { get; set; }

    [BsonElement("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("access_level")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Aluno;
}