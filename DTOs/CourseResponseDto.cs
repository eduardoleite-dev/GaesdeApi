using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CourseResponseDto(
    string Id,
    string Title,
    string Slug,
    string? Description,
    string? CoverImage,
    decimal Price,
    CourseStatus Status,
    CourseLevel Level,
    string InstructorId,
    string? CategoryId,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);