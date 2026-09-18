using Microsoft.AspNetCore.Http;

namespace GaesdeApi.Services.Interfaces;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageOrPdfAsync(IFormFile file);
    Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string publicId, string folder);
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