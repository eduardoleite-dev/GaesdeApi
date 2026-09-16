using Microsoft.AspNetCore.Http;

namespace GaesdeApi.DTOs;

public class UploadMediaRequestDto
{
    public IFormFile? File { get; set; }
}