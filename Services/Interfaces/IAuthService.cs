using GaesdeApi.DTOs;

namespace GaesdeApi.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> AuthenticateAsync(string username, string password);
}