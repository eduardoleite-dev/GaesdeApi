using Microsoft.AspNetCore.Http;

namespace GaesdeApi.Services.Interfaces;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageOrPdfAsync(IFormFile file);
    Task DeleteAsync(string publicId, bool isPdf);
}

public record CloudinaryUploadResult(
    string Url,
    string PublicId,
    string ResourceType,
    string FileName,
    string ContentType,
    long FileSize
);