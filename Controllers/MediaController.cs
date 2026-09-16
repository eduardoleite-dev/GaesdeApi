using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    public MediaController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Professor,Administrador,Vendedor")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] UploadMediaRequestDto request)
    {
        try
        {
            if (request.File is null)
                return BadRequest(new { message = "O arquivo é obrigatório." });

            var result = await _cloudinaryService.UploadImageOrPdfAsync(request.File);
            return Ok(new CloudinaryUploadResponseDto(
                result.Url,
                result.PublicId,
                result.ResourceType,
                result.FileName,
                result.ContentType,
                result.FileSize));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = exception.Message });
        }
    }

    [HttpDelete("{*publicId}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(string publicId, [FromQuery] bool isPdf = false)
    {
        await _cloudinaryService.DeleteAsync(publicId, isPdf);
        return NoContent();
    }
}