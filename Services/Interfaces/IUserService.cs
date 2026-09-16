using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(string id);
    Task<UserResponseDto?> CreateAsync(CreateUserRequestDto request);
    Task<UserResponseDto?> UpdateAsync(string id, UpdateUserRequestDto request);
    Task<bool> DeleteAsync(string id);
}