using GaesdeApi.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_WithDefaultAdmin_ReturnsAdministratorToken()
    {
        var service = CreateService();

        var result = await service.AuthenticateAsync("usuarioMaster", "62270208");

        Assert.NotNull(result);
        Assert.Equal("usuarioMaster", result.Username);
        Assert.Equal("Master", result.UserId);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Equal("0", token.Claims.Single(claim => claim.Type == "nivel_acesso").Value);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidDefaultAdminPassword_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.AuthenticateAsync("usuarioMaster", "senha-incorreta");

        Assert.Null(result);
    }

    private static AuthService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKeyThatIsLongEnoughForJwtSigning"
            })
            .Build();

        var database = new Mock<IMongoDatabase>();
        database
            .Setup(value => value.GetCollection<GaesdeApi.Models.User>("Users", null))
            .Returns(new Mock<IMongoCollection<GaesdeApi.Models.User>>().Object);

        return new AuthService(configuration, database.Object);
    }
}
