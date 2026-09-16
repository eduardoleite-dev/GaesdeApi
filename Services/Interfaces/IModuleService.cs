using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IModuleService
{
    Task<IReadOnlyCollection<ModuleResponseDto>> GetAllAsync(string? courseId = null);
    Task<ModuleResponseDto?> GetByIdAsync(string id);
    Task<ModuleResponseDto?> CreateAsync(string instructorId, bool isAdministrator, CreateModuleRequestDto request);
    Task<ModuleResponseDto?> UpdateAsync(string id, string instructorId, bool isAdministrator, UpdateModuleRequestDto request);
    Task<bool> DeleteAsync(string id, string instructorId, bool isAdministrator);
}