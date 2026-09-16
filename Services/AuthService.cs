using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Services;

public class AuthService : IAuthService
{
    private const string DefaultAdminUsername = "usuarioMaster";
    private const string DefaultAdminPassword = "62270208";
    private const string DefaultAdminId = "Master";

    private readonly IConfiguration _configuration;
    private readonly IMongoCollection<User> _usersCollection;

    public AuthService(IConfiguration configuration, IMongoDatabase database)
    {
        _configuration = configuration;
        _usersCollection = database.GetCollection<User>("Users");
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(string username, string password)
    {
        if (username == DefaultAdminUsername && password == DefaultAdminPassword)
        {
            return GenerateJwtToken(DefaultAdminUsername, DefaultAdminId, AccessLevel.Administrador);
        }

        var user = await _usersCollection
            .Find(u => (u.Email == username || u.Name == username) && u.DeletedAt == null)
            .FirstOrDefaultAsync();
        
        if (user == null) return null; 

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid) return null; 

        var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
        await _usersCollection.UpdateOneAsync(u => u.Id == user.Id, update);

        return GenerateJwtToken(user.Name, user.Id, user.AccessLevel);
    }

    private LoginResponseDto GenerateJwtToken(string username, string userId, AccessLevel accessLevel)
    {
        var secretKey = _configuration["JwtSettings__SecretKey"] 
                        ?? _configuration["JwtSettings:SecretKey"] 
                        ?? "ChavePadraoSuperSecretaFallback123456789";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        
        var expiresAt = DateTime.UtcNow.AddHours(2);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("nivel_acesso", ((int)accessLevel).ToString()),
                new Claim(ClaimTypes.Role, accessLevel.ToString())
            }),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new LoginResponseDto(tokenString, expiresAt, username, userId);
    }
}