using GaesdeApi.Models;

namespace GaesdeApi.DTOs;

public record CreateCourseRequestDto(
    string Title,
    string Slug,
    CourseLevel Level,
    decimal Price = 0,
    string? Description = null,
    string? CoverImage = null,
    string? CategoryId = null
);