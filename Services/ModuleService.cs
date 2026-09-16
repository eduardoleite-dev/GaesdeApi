using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class ModuleService : IModuleService
{
    private readonly IMongoCollection<CourseModule> _modulesCollection;
    private readonly IMongoCollection<Course> _coursesCollection;

    public ModuleService(IMongoDatabase database)
    {
        _modulesCollection = database.GetCollection<CourseModule>("Modules");
        _coursesCollection = database.GetCollection<Course>("Courses");
    }

    public async Task<IReadOnlyCollection<ModuleResponseDto>> GetAllAsync(string? courseId = null)
    {
        var filter = string.IsNullOrWhiteSpace(courseId)
            ? Builders<CourseModule>.Filter.Empty
            : Builders<CourseModule>.Filter.Eq(module => module.CourseId, courseId);

        var modules = await _modulesCollection
            .Find(filter)
            .SortBy(module => module.OrderIndex)
            .ToListAsync();

        return modules.Select(ToResponse).ToArray();
    }

    public async Task<ModuleResponseDto?> GetByIdAsync(string id)
    {
        var module = await _modulesCollection
            .Find(existingModule => existingModule.Id == id)
            .FirstOrDefaultAsync();

        return module is null ? null : ToResponse(module);
    }

    public async Task<ModuleResponseDto?> CreateAsync(
        string instructorId,
        bool isAdministrator,
        CreateModuleRequestDto request)
    {
        if (!IsValid(request.CourseId, request.Title, request.OrderIndex, request.Description) ||
            !await CanManageCourseAsync(request.CourseId, instructorId, isAdministrator) ||
            await OrderExistsAsync(request.CourseId, request.OrderIndex))
            return null;

        var now = DateTime.UtcNow;
        var module = new CourseModule
        {
            CourseId = request.CourseId.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description,
            OrderIndex = request.OrderIndex,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _modulesCollection.InsertOneAsync(module);
        return ToResponse(module);
    }

    public async Task<ModuleResponseDto?> UpdateAsync(
        string id,
        string instructorId,
        bool isAdministrator,
        UpdateModuleRequestDto request)
    {
        var module = await _modulesCollection
            .Find(existingModule => existingModule.Id == id)
            .FirstOrDefaultAsync();

        if (module is null ||
            !IsValid(module.CourseId, request.Title, request.OrderIndex, request.Description) ||
            !await CanManageCourseAsync(module.CourseId, instructorId, isAdministrator) ||
            await OrderExistsAsync(module.CourseId, request.OrderIndex, id))
            return null;

        module.Title = request.Title.Trim();
        module.Description = request.Description;
        module.OrderIndex = request.OrderIndex;
        module.UpdatedAt = DateTime.UtcNow;

        await _modulesCollection.ReplaceOneAsync(existingModule => existingModule.Id == id, module);
        return ToResponse(module);
    }

    public async Task<bool> DeleteAsync(string id, string instructorId, bool isAdministrator)
    {
        var module = await _modulesCollection
            .Find(existingModule => existingModule.Id == id)
            .FirstOrDefaultAsync();

        if (module is null || !await CanManageCourseAsync(module.CourseId, instructorId, isAdministrator))
            return false;

        var result = await _modulesCollection.DeleteOneAsync(existingModule => existingModule.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> CanManageCourseAsync(string courseId, string instructorId, bool isAdministrator)
    {
        return await _coursesCollection.Find(course =>
                course.Id == courseId &&
                course.DeletedAt == null &&
                (isAdministrator || course.InstructorId == instructorId))
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> OrderExistsAsync(string courseId, int orderIndex, string? excludedId = null)
    {
        return await _modulesCollection.Find(module =>
                module.CourseId == courseId &&
                module.OrderIndex == orderIndex &&
                (excludedId == null || module.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private static bool IsValid(string courseId, string title, int orderIndex, string? description)
    {
        return !string.IsNullOrWhiteSpace(courseId) &&
            title.Trim().Length is >= 2 and <= 200 &&
            orderIndex >= 0 &&
            (description is null || description.Length <= 2000);
    }

    private static ModuleResponseDto ToResponse(CourseModule module) => new(
        module.Id,
        module.CourseId,
        module.Title,
        module.Description,
        module.OrderIndex,
        module.CreatedAt,
        module.UpdatedAt);
}