using GaesdeApi.DTOs;
using GaesdeApi.Models;

namespace GaesdeApi.Services.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyCollection<CourseResponseDto>> GetAllAsync();
    Task<IReadOnlyCollection<CourseResponseDto>> GetVisibleAsync(string userId, AccessLevel accessLevel);
    Task<CourseResponseDto?> GetByIdAsync(string id);
    Task<CourseResponseDto?> GetVisibleByIdAsync(string id, string userId, AccessLevel accessLevel);
    Task<CourseResponseDto?> CreateAsync(string instructorId, CreateCourseRequestDto request);
    Task<CourseResponseDto?> UpdateAsync(string id, string instructorId, UpdateCourseRequestDto request);
    Task<CourseResponseDto?> SubmitForReviewAsync(string id, string instructorId);
    Task<CourseResponseDto?> PublishAsync(string id);
    Task<CourseResponseDto?> ArchiveAsync(string id);
    Task<bool> DeleteAsync(string id, string instructorId, bool isAdministrator);
}