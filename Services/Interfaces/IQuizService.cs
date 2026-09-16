using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IQuizService
{
    Task<IReadOnlyCollection<QuizResponseDto>> GetAllAsync();
    Task<QuizResponseDto?> GetByIdAsync(string id);
    Task<QuizResponseDto?> CreateAsync(CreateQuizRequestDto request);
    Task<QuizResponseDto?> UpdateAsync(string id, UpdateQuizRequestDto request);
    Task<bool> DeleteAsync(string id);
}