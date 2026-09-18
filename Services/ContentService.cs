using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class ContentService : IContentService
{
    private readonly IMongoCollection<Content> _contentsCollection;
    private readonly IMongoCollection<CourseModule> _modulesCollection;

    public ContentService(IMongoDatabase database)
    {
        _contentsCollection = database.GetCollection<Content>("Contents");
        _modulesCollection = database.GetCollection<CourseModule>("Modules");
    }

    public async Task<IReadOnlyCollection<ContentResponseDto>> GetAllAsync(string? moduleId = null)
    {
        var filter = string.IsNullOrWhiteSpace(moduleId)
            ? Builders<Content>.Filter.Empty
            : Builders<Content>.Filter.Eq(content => content.ModuleId, moduleId);

        var contents = await _contentsCollection
            .Find(filter)
            .SortBy(content => content.OrderIndex)
            .ToListAsync();

        return contents.Select(ToResponse).ToArray();
    }

    public async Task<ContentResponseDto?> GetByIdAsync(string id)
    {
        var content = await _contentsCollection
            .Find(existingContent => existingContent.Id == id)
            .FirstOrDefaultAsync();

        return content is null ? null : ToResponse(content);
    }

    public async Task<ContentResponseDto?> CreateAsync(CreateContentRequestDto request)
    {
        if (!IsValid(request.ModuleId, request.Title, request.Type, request.OrderIndex,
                request.DurationSeconds, request.FileSizeBytes, request.VideoUrl,
                request.Body, request.FileUrl) ||
            !await ModuleExistsAsync(request.ModuleId) ||
            await OrderExistsAsync(request.ModuleId, request.OrderIndex))
            return null;

        var now = DateTime.UtcNow;
        var content = new Content
        {
            ModuleId = request.ModuleId.Trim(),
            Title = request.Title.Trim(),
            Type = request.Type,
            OrderIndex = request.OrderIndex,
            IsFreePreview = request.IsFreePreview,
            DurationSeconds = request.DurationSeconds,
            VideoUrl = request.VideoUrl,
            Body = request.Body,
            FileUrl = request.FileUrl,
            FileSizeBytes = request.FileSizeBytes,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _contentsCollection.InsertOneAsync(content);
        return ToResponse(content);
    }

    public async Task<ContentResponseDto?> UpdateAsync(string id, UpdateContentRequestDto request)
    {
        var content = await _contentsCollection
            .Find(existingContent => existingContent.Id == id)
            .FirstOrDefaultAsync();

        if (content is null ||
            !IsValid(content.ModuleId, request.Title, request.Type, request.OrderIndex,
                request.DurationSeconds, request.FileSizeBytes, request.VideoUrl,
                request.Body, request.FileUrl) ||
            !await ModuleExistsAsync(content.ModuleId) ||
            await OrderExistsAsync(content.ModuleId, request.OrderIndex, id))
            return null;

        content.Title = request.Title.Trim();
        content.Type = request.Type;
        content.OrderIndex = request.OrderIndex;
        content.IsFreePreview = request.IsFreePreview;
        content.DurationSeconds = request.DurationSeconds;
        content.VideoUrl = request.VideoUrl;
        content.Body = request.Body;
        content.FileUrl = request.FileUrl;
        content.FileSizeBytes = request.FileSizeBytes;
        content.UpdatedAt = DateTime.UtcNow;

        await _contentsCollection.ReplaceOneAsync(existingContent => existingContent.Id == id, content);
        return ToResponse(content);
    }

    public async Task<ContentResponseDto?> UpdatePhotoAsync(string id, string photoUrl)
    {
        var content = await _contentsCollection
            .Find(existingContent => existingContent.Id == id)
            .FirstOrDefaultAsync();

        if (content is null)
            return null;

        content.PhotoUrl = photoUrl;
        content.UpdatedAt = DateTime.UtcNow;
        await _contentsCollection.ReplaceOneAsync(existingContent => existingContent.Id == id, content);
        return ToResponse(content);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _contentsCollection.DeleteOneAsync(content => content.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> OrderExistsAsync(string moduleId, int orderIndex, string? excludedId = null)
    {
        return await _contentsCollection.Find(content =>
                content.ModuleId == moduleId &&
                content.OrderIndex == orderIndex &&
                (excludedId == null || content.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> ModuleExistsAsync(string moduleId)
    {
        return await _modulesCollection.Find(module => module.Id == moduleId)
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(
        string moduleId,
        string title,
        ContentType type,
        int orderIndex,
        int? durationSeconds,
        long? fileSizeBytes,
        string? videoUrl,
        string? body,
        string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(moduleId) || title.Trim().Length is < 2 or > 200 ||
            !Enum.IsDefined(type) || orderIndex < 0 || durationSeconds is < 0 || fileSizeBytes is < 0)
            return false;

        return type switch
        {
            ContentType.Video => IsHttpUrl(videoUrl),
            ContentType.Text => !string.IsNullOrWhiteSpace(body),
            ContentType.Pdf => IsHttpUrl(fileUrl),
            ContentType.Quiz or ContentType.Assignment => true,
            _ => false
        };
    }

    private static bool IsHttpUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static ContentResponseDto ToResponse(Content content) => new(
        content.Id,
        content.ModuleId,
        content.Title,
        content.PhotoUrl,
        content.Type,
        content.OrderIndex,
        content.IsFreePreview,
        content.DurationSeconds,
        content.VideoUrl,
        content.Body,
        content.FileUrl,
        content.FileSizeBytes,
        content.CreatedAt,
        content.UpdatedAt);
}