using GaesdeApi.DTOs;
using GaesdeApi.Models;

namespace GaesdeApi.Services.Interfaces;

public interface ICommentService
{
    Task<IReadOnlyCollection<CommentResponseDto>> GetAllAsync(string userId, AccessLevel accessLevel, string? courseId = null);
    Task<CommentResponseDto?> GetByIdAsync(string id, string userId, AccessLevel accessLevel);
    Task<CommentResponseDto?> CreateAsync(string authorId, AccessLevel authorAccessLevel, CreateCommentRequestDto request);
    Task<CommentResponseDto?> UpdateAsync(string id, string userId, bool isAdministrator, UpdateCommentRequestDto request);
    Task<bool> DeleteAsync(string id, string userId, bool isAdministrator);
    Task<CommentResponseDto?> AddReactionAsync(string id, string userId, CommentReactionRequestDto request);
    Task<CommentResponseDto?> RemoveReactionAsync(string id, string userId);
    Task<CommentResponseDto?> ArchiveAsync(string id, string userId);
    Task<CommentResponseDto?> UnarchiveAsync(string id, string userId);
}