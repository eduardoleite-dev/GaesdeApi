using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IUserAnswerService
{
        Task<IReadOnlyCollection<UserAnswerResponseDto>> GetAllAsync(string? attemptId = null, string? userId = null, bool isAdministrator = false);
        Task<UserAnswerResponseDto?> GetByIdAsync(string id, string? userId = null, bool isAdministrator = false);
        Task<UserAnswerResponseDto?> CreateAsync(CreateUserAnswerRequestDto request, string? userId = null);
        Task<UserAnswerResponseDto?> UpdateAsync(string id, UpdateUserAnswerRequestDto request, string? userId = null, bool isAdministrator = false);
        Task<bool> DeleteAsync(string id, string? userId = null, bool isAdministrator = false);
}