using GaesdeApi.Models;

namespace GaesdeApi.Services.Interfaces;

public interface IEnrollmentAccessService
{
    Task<bool> CanAccessCourseAsync(string userId, string courseId, AccessLevel accessLevel);
    Task<bool> CanAccessModuleAsync(string userId, string moduleId, AccessLevel accessLevel);
    Task<bool> CanAccessQuizAsync(string userId, string quizId, AccessLevel accessLevel);
    Task<bool> CanAccessQuestionAsync(string userId, string questionId, AccessLevel accessLevel);
    Task<bool> CanAccessOptionAsync(string userId, string optionId, AccessLevel accessLevel);
}
