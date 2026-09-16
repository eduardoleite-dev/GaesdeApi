using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IAssignmentSubmissionService
{
    Task<IReadOnlyCollection<AssignmentSubmissionResponseDto>> GetAllAsync(string userId, bool isAdministrator);
    Task<AssignmentSubmissionResponseDto?> GetByIdAsync(string id, string userId, bool isAdministrator);
    Task<AssignmentSubmissionResponseDto?> CreateAsync(string userId, CreateAssignmentSubmissionRequestDto request);
    Task<AssignmentSubmissionResponseDto?> GradeAsync(string id, string userId, bool isAdministrator, GradeAssignmentSubmissionRequestDto request);
    Task<bool> DeleteAsync(string id, string userId, bool isAdministrator);
}