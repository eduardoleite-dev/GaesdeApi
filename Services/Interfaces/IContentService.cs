using GaesdeApi.DTOs;
using GaesdeApi.Models;

namespace GaesdeApi.Services.Interfaces;

public interface IContentService
{
    Task<IReadOnlyCollection<ContentResponseDto>> GetAllAsync(string? moduleId = null, string? userId = null, AccessLevel accessLevel = AccessLevel.Aluno);
    Task<ContentResponseDto?> GetByIdAsync(string id, string? userId = null, AccessLevel accessLevel = AccessLevel.Aluno);
    Task<ContentResponseDto?> CreateAsync(CreateContentRequestDto request);
    Task<ContentResponseDto?> UpdateAsync(string id, UpdateContentRequestDto request);
    Task<ContentResponseDto?> UpdatePhotoAsync(string id, string photoUrl);
    Task<bool> DeleteAsync(string id);
}