using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyCollection<CourseResponseDto>> GetAllAsync();
    Task<CourseResponseDto?> GetByIdAsync(string id);
    Task<CourseResponseDto?> CreateAsync(string instructorId, CreateCourseRequestDto request);
    Task<CourseResponseDto?> UpdateAsync(string id, string instructorId, UpdateCourseRequestDto request);
    Task<CourseResponseDto?> PublishAsync(string id);
    Task<CourseResponseDto?> ArchiveAsync(string id);
    Task<bool> DeleteAsync(string id, string instructorId, bool isAdministrator);
}