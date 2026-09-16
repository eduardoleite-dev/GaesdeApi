using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record UpdateCourseRequestDto(
    string Title,
    string Slug,
    CourseLevel Level,
    decimal Price,
    string? Description = null,
    string? CoverImage = null,
    string? CategoryId = null
);