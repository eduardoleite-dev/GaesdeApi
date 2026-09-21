using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IContentCompletionService
{
    Task<ContentCompletionResponseDto?> CompleteAsync(string userId, string contentId);
    Task<bool> UncompleteAsync(string userId, string contentId);
    Task<ContentCompletionResponseDto?> GetAsync(string userId, string contentId);
    Task<CourseProgressResponseDto?> GetProgressAsync(string userId, string courseId);
}
