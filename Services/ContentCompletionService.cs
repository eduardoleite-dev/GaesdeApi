using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class ContentCompletionService : IContentCompletionService
{
    private readonly IMongoCollection<ContentCompletion> _completionsCollection;
    private readonly IMongoCollection<Content> _contentsCollection;
    private readonly IMongoCollection<CourseModule> _modulesCollection;
    private readonly IMongoCollection<Enrollment> _enrollmentsCollection;

    public ContentCompletionService(IMongoDatabase database)
    {
        _completionsCollection = database.GetCollection<ContentCompletion>("ContentCompletions");
        _contentsCollection = database.GetCollection<Content>("Contents");
        _modulesCollection = database.GetCollection<CourseModule>("Modules");
        _enrollmentsCollection = database.GetCollection<Enrollment>("Enrollments");
    }

    public async Task<ContentCompletionResponseDto?> CompleteAsync(string userId, string contentId)
    {
        var content = await _contentsCollection.Find(value => value.Id == contentId).FirstOrDefaultAsync();
        if (content is null)
            return null;

        var module = await _modulesCollection.Find(value => value.Id == content.ModuleId).FirstOrDefaultAsync();
        if (module is null)
            return null;

        var enrollment = await FindActiveEnrollmentAsync(userId, module.CourseId);
        if (enrollment is null || !await IsPreviousContentCompleteAsync(userId, module, content))
            return null;

        var existing = await _completionsCollection.Find(value =>
                value.UserId == userId && value.ContentId == contentId)
            .FirstOrDefaultAsync();
        if (existing is not null)
            return ToResponse(existing);

        var completion = new ContentCompletion
        {
            UserId = userId,
            ContentId = contentId,
            CompletedAt = DateTime.UtcNow
        };
        await _completionsCollection.InsertOneAsync(completion);
        await UpdateProgressAsync(enrollment, module.CourseId, userId);
        return ToResponse(completion);
    }

    public async Task<bool> UncompleteAsync(string userId, string contentId)
    {
        var result = await _completionsCollection.DeleteOneAsync(value =>
            value.UserId == userId && value.ContentId == contentId);
        if (result.DeletedCount > 0)
        {
            var content = await _contentsCollection.Find(value => value.Id == contentId).FirstOrDefaultAsync();
            if (content is not null)
            {
                var module = await _modulesCollection.Find(value => value.Id == content.ModuleId).FirstOrDefaultAsync();
                var enrollment = module is null ? null : await FindActiveEnrollmentAsync(userId, module.CourseId);
                if (module is not null && enrollment is not null)
                    await UpdateProgressAsync(enrollment, module.CourseId, userId);
            }
        }
        return result.DeletedCount > 0;
    }

    public async Task<ContentCompletionResponseDto?> GetAsync(string userId, string contentId)
    {
        var completion = await _completionsCollection.Find(value =>
                value.UserId == userId && value.ContentId == contentId)
            .FirstOrDefaultAsync();
        return completion is null ? null : ToResponse(completion);
    }

    public async Task<CourseProgressResponseDto?> GetProgressAsync(string userId, string courseId)
    {
        var enrollment = await FindActiveEnrollmentAsync(userId, courseId);
        if (enrollment is null)
            return null;

        return await UpdateProgressAsync(enrollment, courseId, userId);
    }

    private async Task<CourseProgressResponseDto> UpdateProgressAsync(
        Enrollment enrollment,
        string courseId,
        string userId)
    {
        var modules = await _modulesCollection.Find(value => value.CourseId == courseId)
            .SortBy(value => value.OrderIndex)
            .ToListAsync();
        var moduleIds = modules.Select(value => value.Id).ToArray();
        var contents = moduleIds.Length == 0
            ? new List<Content>()
            : await _contentsCollection.Find(value => moduleIds.Contains(value.ModuleId))
                .SortBy(value => value.OrderIndex)
                .ToListAsync();
        var contentIds = contents.Select(value => value.Id).ToArray();
        var completed = contentIds.Length == 0
            ? new List<ContentCompletion>()
            : await _completionsCollection.Find(value =>
                    value.UserId == userId && contentIds.Contains(value.ContentId))
                .ToListAsync();
        var completedIds = completed.Select(value => value.ContentId).ToHashSet();
        var percentage = contents.Count == 0
            ? 0
            : Math.Round(completedIds.Count * 100m / contents.Count, 2);

        enrollment.ProgressPercentage = percentage;
        enrollment.LastAccessedAt = DateTime.UtcNow;
        enrollment.Status = percentage == 100 ? EnrollmentStatus.Completed : EnrollmentStatus.Active;
        await _enrollmentsCollection.ReplaceOneAsync(value => value.Id == enrollment.Id, enrollment);

        return new CourseProgressResponseDto(
            enrollment.Id,
            courseId,
            contents.Count,
            completedIds.Count,
            percentage,
            completedIds.ToArray());
    }

    private async Task<Enrollment?> FindActiveEnrollmentAsync(string userId, string courseId) =>
        await _enrollmentsCollection.Find(value =>
                value.UserId == userId &&
                value.CourseId == courseId &&
                (value.Status == EnrollmentStatus.Active || value.Status == EnrollmentStatus.Completed) &&
                (value.ExpiresAt == null || value.ExpiresAt > DateTime.UtcNow))
            .FirstOrDefaultAsync();

    private async Task<bool> IsPreviousContentCompleteAsync(
        string userId,
        CourseModule module,
        Content content)
    {
        var modules = await _modulesCollection.Find(value => value.CourseId == module.CourseId)
            .SortBy(value => value.OrderIndex)
            .ToListAsync();
        var priorModuleIds = modules
            .Where(value => value.OrderIndex < module.OrderIndex)
            .Select(value => value.Id)
            .ToArray();
        var priorContents = await _contentsCollection.Find(value =>
                (priorModuleIds.Contains(value.ModuleId) ||
                 value.ModuleId == module.Id && value.OrderIndex < content.OrderIndex))
            .ToListAsync();
        if (priorContents.Count == 0)
            return true;

        var priorIds = priorContents.Select(value => value.Id).ToArray();
        var completedCount = await _completionsCollection.CountDocumentsAsync(value =>
            value.UserId == userId && priorIds.Contains(value.ContentId));
        return completedCount == priorIds.Length;
    }

    private static ContentCompletionResponseDto ToResponse(ContentCompletion completion) => new(
        completion.Id,
        completion.UserId,
        completion.ContentId,
        completion.CompletedAt);
}
