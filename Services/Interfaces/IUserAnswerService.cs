using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IUserAnswerService
{
    Task<IReadOnlyCollection<UserAnswerResponseDto>> GetAllAsync(string? attemptId = null);
    Task<UserAnswerResponseDto?> GetByIdAsync(string id);
    Task<UserAnswerResponseDto?> CreateAsync(CreateUserAnswerRequestDto request);
    Task<UserAnswerResponseDto?> UpdateAsync(string id, UpdateUserAnswerRequestDto request);
    Task<bool> DeleteAsync(string id);
}