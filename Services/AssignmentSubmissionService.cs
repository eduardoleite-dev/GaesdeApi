using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class AssignmentSubmissionService : IAssignmentSubmissionService
{
    private readonly IMongoCollection<AssignmentSubmission> _submissionsCollection;
    private readonly IMongoCollection<Content> _contentsCollection;
    private readonly IMongoCollection<Enrollment> _enrollmentsCollection;
    private readonly IMongoCollection<CourseModule> _modulesCollection;
    private readonly IMongoCollection<Course> _coursesCollection;

    public AssignmentSubmissionService(IMongoDatabase database)
    {
        _submissionsCollection = database.GetCollection<AssignmentSubmission>("AssignmentSubmissions");
        _contentsCollection = database.GetCollection<Content>("Contents");
        _enrollmentsCollection = database.GetCollection<Enrollment>("Enrollments");
        _modulesCollection = database.GetCollection<CourseModule>("Modules");
        _coursesCollection = database.GetCollection<Course>("Courses");
    }

    public async Task<IReadOnlyCollection<AssignmentSubmissionResponseDto>> GetAllAsync(string userId, bool isAdministrator)
    {
        var filter = isAdministrator
            ? Builders<AssignmentSubmission>.Filter.Empty
            : Builders<AssignmentSubmission>.Filter.Eq(submission => submission.UserId, userId);

        var submissions = await _submissionsCollection.Find(filter)
            .SortByDescending(submission => submission.SubmittedAt)
            .ToListAsync();

        return submissions.Select(ToResponse).ToArray();
    }

    public async Task<AssignmentSubmissionResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator)
    {
        var filter = Builders<AssignmentSubmission>.Filter.Eq(submission => submission.Id, id);
        if (!isAdministrator)
            filter &= Builders<AssignmentSubmission>.Filter.Eq(submission => submission.UserId, userId);

        var submission = await _submissionsCollection.Find(filter).FirstOrDefaultAsync();
        return submission is null ? null : ToResponse(submission);
    }

    public async Task<AssignmentSubmissionResponseDto?> CreateAsync(
        string userId,
        CreateAssignmentSubmissionRequestDto request)
    {
        if (!IsHttpUrl(request.FileUrl) ||
            !await IsAssignmentContentAsync(request.ContentId) ||
            !await ActiveEnrollmentBelongsToUserAsync(request.EnrollmentId, userId) ||
            !await EnrollmentMatchesCourseAsync(request.EnrollmentId, request.ContentId) ||
            await SubmissionExistsAsync(request.EnrollmentId, request.ContentId))
            return null;

        var submission = new AssignmentSubmission
        {
            ContentId = request.ContentId.Trim(),
            UserId = userId,
            EnrollmentId = request.EnrollmentId.Trim(),
            FileUrl = request.FileUrl.Trim(),
            SubmittedAt = DateTime.UtcNow
        };

        await _submissionsCollection.InsertOneAsync(submission);
        return ToResponse(submission);
    }

    public async Task<AssignmentSubmissionResponseDto?> GradeAsync(
        string id,
        string userId,
        bool isAdministrator,
        GradeAssignmentSubmissionRequestDto request)
    {
        if (request.Grade is < 0 or > 100)
            return null;

        var submission = await _submissionsCollection.Find(
            existingSubmission => existingSubmission.Id == id)
            .FirstOrDefaultAsync();

        if (submission is null || submission.Grade.HasValue ||
            !await CanGradeAsync(submission.ContentId, userId, isAdministrator))
            return null;

        submission.Grade = request.Grade;
        submission.InstructorFeedback = request.InstructorFeedback;
        submission.GradedAt = DateTime.UtcNow;

        await _submissionsCollection.ReplaceOneAsync(existingSubmission => existingSubmission.Id == id, submission);
        return ToResponse(submission);
    }

    public async Task<bool> DeleteAsync(string id, string userId, bool isAdministrator)
    {
        var filter = isAdministrator
            ? Builders<AssignmentSubmission>.Filter.Eq(submission => submission.Id, id)
            : Builders<AssignmentSubmission>.Filter.And(
                Builders<AssignmentSubmission>.Filter.Eq(submission => submission.Id, id),
                Builders<AssignmentSubmission>.Filter.Eq(submission => submission.UserId, userId));

        return (await _submissionsCollection.DeleteOneAsync(filter)).DeletedCount > 0;
    }

    private async Task<bool> IsAssignmentContentAsync(string contentId) =>
        await _contentsCollection.Find(content => content.Id == contentId && content.Type == ContentType.Assignment)
            .Limit(1).AnyAsync();

    private async Task<bool> ActiveEnrollmentBelongsToUserAsync(string enrollmentId, string userId) =>
        await _enrollmentsCollection.Find(enrollment =>
                enrollment.Id == enrollmentId &&
                enrollment.UserId == userId &&
                enrollment.Status == EnrollmentStatus.Active &&
                (enrollment.ExpiresAt == null || enrollment.ExpiresAt > DateTime.UtcNow))
            .Limit(1).AnyAsync();

    private async Task<bool> EnrollmentMatchesCourseAsync(string enrollmentId, string contentId)
    {
        var enrollment = await _enrollmentsCollection.Find(value => value.Id == enrollmentId).FirstOrDefaultAsync();
        var content = await _contentsCollection.Find(value => value.Id == contentId).FirstOrDefaultAsync();
        if (enrollment is null || content is null)
            return false;

        var module = await _modulesCollection.Find(value => value.Id == content.ModuleId).FirstOrDefaultAsync();
        return module is not null && module.CourseId == enrollment.CourseId;
    }

    private async Task<bool> SubmissionExistsAsync(string enrollmentId, string contentId) =>
        await _submissionsCollection.Find(submission =>
                submission.EnrollmentId == enrollmentId && submission.ContentId == contentId)
            .Limit(1).AnyAsync();

    private async Task<bool> CanGradeAsync(string contentId, string userId, bool isAdministrator)
    {
        if (isAdministrator)
            return true;

        var content = await _contentsCollection.Find(value => value.Id == contentId).FirstOrDefaultAsync();
        if (content is null)
            return false;
        var module = await _modulesCollection.Find(value => value.Id == content.ModuleId).FirstOrDefaultAsync();
        return module is not null && await _coursesCollection.Find(course =>
                course.Id == module.CourseId && course.InstructorId == userId)
            .Limit(1).AnyAsync();
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static AssignmentSubmissionResponseDto ToResponse(AssignmentSubmission submission) => new(
        submission.Id,
        submission.ContentId,
        submission.UserId,
        submission.EnrollmentId,
        submission.FileUrl,
        submission.SubmittedAt,
        submission.Grade,
        submission.InstructorFeedback,
        submission.GradedAt,
        submission.Grade.HasValue);
}