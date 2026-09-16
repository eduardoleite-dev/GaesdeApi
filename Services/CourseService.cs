using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class CourseService : ICourseService
{
    private readonly IMongoCollection<Course> _coursesCollection;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<Category> _categoriesCollection;

    public CourseService(IMongoDatabase database)
    {
        _coursesCollection = database.GetCollection<Course>("Courses");
        _usersCollection = database.GetCollection<User>("Users");
        _categoriesCollection = database.GetCollection<Category>("Categories");
    }

    public async Task<IReadOnlyCollection<CourseResponseDto>> GetAllAsync()
    {
        var courses = await _coursesCollection
            .Find(course => course.DeletedAt == null)
            .SortBy(course => course.Title)
            .ToListAsync();

        return courses.Select(ToResponse).ToArray();
    }

    public async Task<CourseResponseDto?> GetByIdAsync(string id)
    {
        var course = await _coursesCollection
            .Find(course => course.Id == id && course.DeletedAt == null)
            .FirstOrDefaultAsync();

        return course is null ? null : ToResponse(course);
    }

    public async Task<CourseResponseDto?> CreateAsync(string instructorId, CreateCourseRequestDto request)
    {
        if (!Enum.IsDefined(request.Level) ||
            !IsValidContent(request.Title, request.Slug, request.Price, request.Description) ||
            !await IsProfessorAsync(instructorId) ||
            await SlugExistsAsync(request.Slug) ||
            !await CategoryExistsAsync(request.CategoryId))
            return null;

        var now = DateTime.UtcNow;
        var course = new Course
        {
            Title = request.Title.Trim(),
            Slug = request.Slug.Trim(),
            Description = request.Description,
            CoverImage = request.CoverImage,
            Price = request.Price,
            Level = request.Level,
            InstructorId = instructorId,
            CategoryId = request.CategoryId,
            Status = CourseStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _coursesCollection.InsertOneAsync(course);
        return ToResponse(course);
    }

    public async Task<CourseResponseDto?> UpdateAsync(string id, string instructorId, UpdateCourseRequestDto request)
    {
        if (!Enum.IsDefined(request.Level) ||
            !IsValidContent(request.Title, request.Slug, request.Price, request.Description) ||
            !await CategoryExistsAsync(request.CategoryId))
            return null;

        var course = await _coursesCollection
            .Find(existingCourse =>
                existingCourse.Id == id &&
                existingCourse.InstructorId == instructorId &&
                existingCourse.DeletedAt == null)
            .FirstOrDefaultAsync();

        if (course is null || await SlugExistsAsync(request.Slug, id))
            return null;

        course.Title = request.Title.Trim();
        course.Slug = request.Slug.Trim();
        course.Description = request.Description;
        course.CoverImage = request.CoverImage;
        course.Price = request.Price;
        course.Level = request.Level;
        course.CategoryId = request.CategoryId;
        course.UpdatedAt = DateTime.UtcNow;

        await _coursesCollection.ReplaceOneAsync(existingCourse => existingCourse.Id == id, course);
        return ToResponse(course);
    }

    public async Task<CourseResponseDto?> PublishAsync(string id)
    {
        var course = await GetActiveCourseAsync(id);
        if (course is null || course.Status == CourseStatus.Published)
            return null;

        course.Status = CourseStatus.Published;
        course.PublishedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        await _coursesCollection.ReplaceOneAsync(existingCourse => existingCourse.Id == id, course);
        return ToResponse(course);
    }

    public async Task<CourseResponseDto?> ArchiveAsync(string id)
    {
        var course = await GetActiveCourseAsync(id);
        if (course is null || course.Status == CourseStatus.Archived)
            return null;

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTime.UtcNow;
        await _coursesCollection.ReplaceOneAsync(existingCourse => existingCourse.Id == id, course);
        return ToResponse(course);
    }

    public async Task<bool> DeleteAsync(string id, string instructorId, bool isAdministrator)
    {
        var filter = isAdministrator
            ? Builders<Course>.Filter.Eq(course => course.Id, id)
            : Builders<Course>.Filter.And(
                Builders<Course>.Filter.Eq(course => course.Id, id),
                Builders<Course>.Filter.Eq(course => course.InstructorId, instructorId));

        var update = Builders<Course>.Update
            .Set(course => course.DeletedAt, DateTime.UtcNow)
            .Set(course => course.UpdatedAt, DateTime.UtcNow);

        var result = await _coursesCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    private async Task<bool> IsProfessorAsync(string instructorId)
    {
        return await _usersCollection.Find(user =>
                user.Id == instructorId &&
                user.AccessLevel == AccessLevel.Professor &&
                user.DeletedAt == null)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> CategoryExistsAsync(string? categoryId)
    {
        return string.IsNullOrWhiteSpace(categoryId) || await _categoriesCollection
            .Find(category => category.Id == categoryId)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> SlugExistsAsync(string slug, string? excludedId = null)
    {
        return await _coursesCollection.Find(course =>
                course.Slug == slug.Trim() &&
                course.DeletedAt == null &&
                (excludedId == null || course.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private async Task<Course?> GetActiveCourseAsync(string id)
    {
        return await _coursesCollection
            .Find(course => course.Id == id && course.DeletedAt == null)
            .FirstOrDefaultAsync();
    }

    private static bool IsValidContent(string title, string slug, decimal price, string? description)
    {
        return title.Trim().Length is >= 3 and <= 200 &&
            slug.Trim().Length >= 3 &&
            slug.All(character => char.IsLower(character) || char.IsDigit(character) || character == '-') &&
            price >= 0 &&
            (description is null || description.Length <= 5000);
    }

    private static CourseResponseDto ToResponse(Course course) => new(
        course.Id,
        course.Title,
        course.Slug,
        course.Description,
        course.CoverImage,
        course.Price,
        course.Status,
        course.Level,
        course.InstructorId,
        course.CategoryId,
        course.PublishedAt,
        course.CreatedAt,
        course.UpdatedAt);
}