using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace GaesdeApi.Services;

public class CloudinaryService : ICloudinaryService
{
    private const long MaxFileSize = 20 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        var value = settings.Value;
        if (string.IsNullOrWhiteSpace(value.CloudName) ||
            string.IsNullOrWhiteSpace(value.ApiKey) ||
            string.IsNullOrWhiteSpace(value.ApiSecret))
            throw new InvalidOperationException("CloudinarySettings não está configurado corretamente.");

        _cloudinary = new Cloudinary(new Account(value.CloudName, value.ApiKey, value.ApiSecret));
    }

    public async Task<CloudinaryUploadResult> UploadImageOrPdfAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("O arquivo é obrigatório.", nameof(file));
        if (file.Length > MaxFileSize)
            throw new ArgumentException("O arquivo não pode exceder 20 MB.", nameof(file));

        var isPdf = string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                    Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        var isImage = AllowedImageTypes.Contains(file.ContentType);
        if (!isPdf && !isImage)
            throw new ArgumentException("Apenas imagens JPG, PNG, WEBP, GIF ou PDF são permitidos.", nameof(file));

        await using var stream = file.OpenReadStream();
        var fileDescription = new FileDescription(file.FileName, stream);
        UploadResult result;

        if (isPdf)
        {
            result = await _cloudinary.UploadAsync(new RawUploadParams
            {
                File = fileDescription,
                Folder = "gaesde/pdfs",
                PublicId = Path.GetFileNameWithoutExtension(file.FileName)
            });
        }
        else
        {
            result = await _cloudinary.UploadAsync(new ImageUploadParams
            {
                File = fileDescription,
                Folder = "gaesde/images",
                PublicId = Path.GetFileNameWithoutExtension(file.FileName)
            });
        }

        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
            throw new InvalidOperationException(result.Error?.Message ?? "Falha ao enviar arquivo para o Cloudinary.");

        return new CloudinaryUploadResult(
            result.SecureUrl.ToString(),
            result.PublicId,
            isPdf ? "raw" : "image",
            file.FileName,
            file.ContentType,
            file.Length);
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string publicId, string folder)
    {
        ValidateImage(file);

        await using var stream = file.OpenReadStream();
        var result = await _cloudinary.UploadAsync(new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            PublicId = $"{publicId}{Path.GetExtension(file.FileName).ToLowerInvariant()}",
            Overwrite = true
        });

        if (result.Error is not null || string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
            throw new InvalidOperationException(result.Error?.Message ?? "Falha ao enviar imagem para o Cloudinary.");

        return new CloudinaryUploadResult(
            result.SecureUrl.ToString(),
            result.PublicId,
            "image",
            file.FileName,
            file.ContentType,
            file.Length);
    }

    public async Task DeleteAsync(string publicId, bool isPdf)
    {
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
        {
            ResourceType = isPdf ? ResourceType.Raw : ResourceType.Image
        });

        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);
    }

    private static void ValidateImage(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("A imagem é obrigatória.", nameof(file));
        if (file.Length > MaxFileSize)
            throw new ArgumentException("A imagem não pode exceder 20 MB.", nameof(file));
        if (!AllowedImageTypes.Contains(file.ContentType))
            throw new ArgumentException("Apenas imagens JPG, PNG, WEBP ou GIF são permitidas.", nameof(file));
    }

    public static string CreatePublicId(string type, string name, string id)
    {
        var normalizedName = name.Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (builder, character) => builder.Append(character))
            .ToString()
            .ToLowerInvariant();
        var slug = new string(normalizedName
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());
        while (slug.Contains("--", StringComparison.Ordinal))
            slug = slug.Replace("--", "-", StringComparison.Ordinal);

        return $"{type}-{slug.Trim('-')}-{id}";
    }
}

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}