using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IQuestionOptionService
{
    Task<IReadOnlyCollection<QuestionOptionResponseDto>> GetAllAsync(string? questionId = null);
    Task<QuestionOptionResponseDto?> GetByIdAsync(string id);
    Task<QuestionOptionResponseDto?> CreateAsync(CreateQuestionOptionRequestDto request);
    Task<QuestionOptionResponseDto?> UpdateAsync(string id, UpdateQuestionOptionRequestDto request);
    Task<bool> DeleteAsync(string id);
}