using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IContentService
{
    Task<IReadOnlyCollection<ContentResponseDto>> GetAllAsync(string? moduleId = null);
    Task<ContentResponseDto?> GetByIdAsync(string id);
    Task<ContentResponseDto?> CreateAsync(CreateContentRequestDto request);
    Task<ContentResponseDto?> UpdateAsync(string id, UpdateContentRequestDto request);
    Task<bool> DeleteAsync(string id);
}