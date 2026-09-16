using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserService(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("Users");
    }

    public async Task<IReadOnlyCollection<UserResponseDto>> GetAllAsync()
    {
        var users = await _usersCollection
            .Find(user => user.DeletedAt == null)
            .SortBy(user => user.Name)
            .ToListAsync();

        return users.Select(ToResponse).ToArray();
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id)
    {
        var user = await _usersCollection
            .Find(user => user.Id == id && user.DeletedAt == null)
            .FirstOrDefaultAsync();

        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponseDto?> CreateAsync(CreateUserRequestDto request)
    {
        if (!Enum.IsDefined(request.AccessLevel) || await EmailExistsAsync(request.Email))
            return null;

        var now = DateTime.UtcNow;
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            AvatarUrl = request.AvatarUrl,
            Bio = request.Bio,
            AccessLevel = request.AccessLevel,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _usersCollection.InsertOneAsync(user);
        return ToResponse(user);
    }

    public async Task<UserResponseDto?> UpdateAsync(string id, UpdateUserRequestDto request)
    {
        if (!Enum.IsDefined(request.AccessLevel))
            return null;

        var user = await _usersCollection
            .Find(existingUser => existingUser.Id == id && existingUser.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (user is null || await EmailExistsAsync(request.Email, id))
            return null;

        user.Name = request.Name.Trim();
        user.Email = request.Email.Trim();
        user.AvatarUrl = request.AvatarUrl;
        user.Bio = request.Bio;
        user.AccessLevel = request.AccessLevel;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _usersCollection.ReplaceOneAsync(existingUser => existingUser.Id == id, user);
        return ToResponse(user);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<User>.Update
            .Set(user => user.DeletedAt, DateTime.UtcNow)
            .Set(user => user.UpdatedAt, DateTime.UtcNow);

        var result = await _usersCollection.UpdateOneAsync(
            user => user.Id == id && user.DeletedAt == null,
            update);

        return result.ModifiedCount > 0;
    }

    private async Task<bool> EmailExistsAsync(string email, string? excludedId = null)
    {
        return await _usersCollection.Find(user =>
                user.Email == email.Trim() &&
                user.DeletedAt == null &&
                (excludedId == null || user.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private static UserResponseDto ToResponse(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.AvatarUrl,
        user.Bio,
        user.EmailVerifiedAt,
        user.LastLoginAt,
        user.CreatedAt,
        user.UpdatedAt,
        user.AccessLevel);
}