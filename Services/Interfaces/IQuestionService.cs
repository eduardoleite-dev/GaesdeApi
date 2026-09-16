using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IQuestionService
{
    Task<IReadOnlyCollection<QuestionResponseDto>> GetAllAsync(string? quizId = null);
    Task<QuestionResponseDto?> GetByIdAsync(string id);
    Task<QuestionResponseDto?> CreateAsync(CreateQuestionRequestDto request);
    Task<QuestionResponseDto?> UpdateAsync(string id, UpdateQuestionRequestDto request);
    Task<bool> DeleteAsync(string id);
}