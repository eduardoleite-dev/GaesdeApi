using GaesdeApi.DTOs;
using GaesdeApi.Models;

namespace GaesdeApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyCollection<EnrollmentResponseDto>> GetAllAsync(string userId, bool isAdministrator);
    Task<EnrollmentResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator);
    Task<EnrollmentResponseDto?> CreateAsync(string requesterId, bool isAdministrator, CreateEnrollmentRequestDto request);
    Task<EnrollmentResponseDto?> UpdateProgressAsync(string id, string userId, bool isAdministrator, decimal progressPercentage);
    Task<EnrollmentResponseDto?> UpdateStatusAsync(string id, string userId, bool isAdministrator, EnrollmentStatus status);
    Task<bool> DeleteAsync(string id, string userId, bool isAdministrator);
}