using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IQuizAttemptService
{
    Task<QuizAttemptResponseDto?> StartAsync(string userId, StartQuizAttemptRequestDto request);
    Task<IReadOnlyCollection<QuizAttemptResponseDto>> GetAllAsync(string userId, bool isAdministrator, string? quizId = null);
    Task<QuizAttemptResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator);
    Task<QuizAttemptResponseDto?> FinishAsync(string id, string userId, bool isAdministrator);
    Task<QuizAttemptResponseDto?> AbandonAsync(string id, string userId, bool isAdministrator);
}
