namespace GaesdeApi.DTOs;

public record CourseProgressResponseDto(
    string EnrollmentId,
    string CourseId,
    int TotalContents,
    int CompletedContents,
    decimal ProgressPercentage,
    IReadOnlyCollection<string> CompletedContentIds
);
