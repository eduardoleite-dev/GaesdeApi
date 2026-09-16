using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IMongoCollection<Enrollment> _enrollmentsCollection;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IMongoCollection<Course> _coursesCollection;

    public EnrollmentService(IMongoDatabase database)
    {
        _enrollmentsCollection = database.GetCollection<Enrollment>("Enrollments");
        _usersCollection = database.GetCollection<User>("Users");
        _coursesCollection = database.GetCollection<Course>("Courses");
    }

    public async Task<IReadOnlyCollection<EnrollmentResponseDto>> GetAllAsync(string userId, bool isAdministrator)
    {
        var filter = isAdministrator
            ? Builders<Enrollment>.Filter.Empty
            : Builders<Enrollment>.Filter.Eq(enrollment => enrollment.UserId, userId);

        var enrollments = await _enrollmentsCollection
            .Find(filter)
            .SortByDescending(enrollment => enrollment.EnrolledAt)
            .ToListAsync();

        return enrollments.Select(ToResponse).ToArray();
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<Enrollment>.Filter.Eq(enrollment => enrollment.Id, id);
        if (!isAdministrator)
            filter &= Builders<Enrollment>.Filter.Eq(enrollment => enrollment.UserId, userId);

        var enrollment = await _enrollmentsCollection.Find(filter).FirstOrDefaultAsync();
        return enrollment is null ? null : ToResponse(enrollment);
    }

    public async Task<EnrollmentResponseDto?> CreateAsync(
        string requesterId,
        bool isAdministrator,
        CreateEnrollmentRequestDto request)
    {
        var userId = isAdministrator && !string.IsNullOrWhiteSpace(request.UserId)
            ? request.UserId
            : requesterId;

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(request.CourseId) ||
            !await UserExistsAsync(userId) ||
            !await CourseExistsAsync(request.CourseId) ||
            await EnrollmentExistsAsync(userId, request.CourseId))
            return null;

        var now = DateTime.UtcNow;
        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = request.CourseId,
            Status = EnrollmentStatus.Active,
            ProgressPercentage = 0,
            EnrolledAt = now,
            ExpiresAt = request.ExpiresAt,
            LastAccessedAt = now
        };

        await _enrollmentsCollection.InsertOneAsync(enrollment);
        return ToResponse(enrollment);
    }

    public async Task<EnrollmentResponseDto?> UpdateProgressAsync(
        string id,
        string userId,
        bool isAdministrator,
        decimal progressPercentage)
    {
        if (progressPercentage is < 0 or > 100)
            return null;

        var enrollment = await FindAuthorizedAsync(id, userId, isAdministrator);
        if (enrollment is null)
            return null;

        enrollment.ProgressPercentage = progressPercentage;
        enrollment.LastAccessedAt = DateTime.UtcNow;
        if (progressPercentage == 100)
            enrollment.Status = EnrollmentStatus.Completed;

        await _enrollmentsCollection.ReplaceOneAsync(existingEnrollment => existingEnrollment.Id == id, enrollment);
        return ToResponse(enrollment);
    }

    public async Task<EnrollmentResponseDto?> UpdateStatusAsync(
        string id,
        string userId,
        bool isAdministrator,
        EnrollmentStatus status)
    {
        if (!Enum.IsDefined(status))
            return null;

        var enrollment = await FindAuthorizedAsync(id, userId, isAdministrator);
        if (enrollment is null)
            return null;

        enrollment.Status = status;
        enrollment.LastAccessedAt = DateTime.UtcNow;
        if (status == EnrollmentStatus.Completed)
            enrollment.ProgressPercentage = 100;

        await _enrollmentsCollection.ReplaceOneAsync(existingEnrollment => existingEnrollment.Id == id, enrollment);
        return ToResponse(enrollment);
    }

    public async Task<bool> DeleteAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<Enrollment>.Filter.Eq(enrollment => enrollment.Id, id);
        if (!isAdministrator)
            filter &= Builders<Enrollment>.Filter.Eq(enrollment => enrollment.UserId, userId);

        var result = await _enrollmentsCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    private async Task<Enrollment?> FindAuthorizedAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<Enrollment>.Filter.Eq(enrollment => enrollment.Id, id);
        if (!isAdministrator)
            filter &= Builders<Enrollment>.Filter.Eq(enrollment => enrollment.UserId, userId);

        return await _enrollmentsCollection.Find(filter).FirstOrDefaultAsync();
    }

    private async Task<bool> UserExistsAsync(string userId)
    {
        return await _usersCollection.Find(user => user.Id == userId && user.DeletedAt == null)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> CourseExistsAsync(string courseId)
    {
        return await _coursesCollection.Find(course => course.Id == courseId && course.DeletedAt == null)
            .Limit(1)
            .AnyAsync();
    }

    private async Task<bool> EnrollmentExistsAsync(string userId, string courseId)
    {
        return await _enrollmentsCollection.Find(enrollment =>
                enrollment.UserId == userId && enrollment.CourseId == courseId)
            .Limit(1)
            .AnyAsync();
    }

    private static EnrollmentResponseDto ToResponse(Enrollment enrollment)
    {
        var isExpired = enrollment.ExpiresAt.HasValue && DateTime.UtcNow > enrollment.ExpiresAt.Value;
        return new EnrollmentResponseDto(
            enrollment.Id,
            enrollment.UserId,
            enrollment.CourseId,
            enrollment.Status,
            enrollment.ProgressPercentage,
            enrollment.EnrolledAt,
            enrollment.ExpiresAt,
            enrollment.LastAccessedAt,
            enrollment.Status == EnrollmentStatus.Active && !isExpired,
            enrollment.Status == EnrollmentStatus.Completed,
            isExpired);
    }
}