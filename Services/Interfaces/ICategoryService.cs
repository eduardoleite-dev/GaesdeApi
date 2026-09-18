using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto?> GetByIdAsync(string id);
    Task<CategoryResponseDto?> CreateAsync(CreateCategoryRequestDto request);
    Task<CategoryResponseDto?> UpdateAsync(string id, UpdateCategoryRequestDto request);
    Task<CategoryResponseDto?> UpdateImageAsync(string id, string imageUrl);
    Task<bool> DeleteAsync(string id);
}