using System.Security.Claims;
using GaesdeApi;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GaesdeApi.Tests;

public class UtilsTests
{
    [Fact]
    public void CheckAccessLevel_WithMatchingClaim_ReturnsOk()
    {
        var controller = new ControllerBaseForTest();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("nivel_acesso", "2") }, "Test"));

        var result = Utils.CheckAccessLevel(user, controller, 2, "Hello");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void CheckAccessLevel_WithWrongClaim_ReturnsForbid()
    {
        var controller = new ControllerBaseForTest();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("nivel_acesso", "3") }, "Test"));

        var result = Utils.CheckAccessLevel(user, controller, 2, "Hello");

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void CheckAccessLevel_WithInvalidClaim_ReturnsForbid()
    {
        var controller = new ControllerBaseForTest();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("nivel_acesso", "invalid") }, "Test"));

        var result = Utils.CheckAccessLevel(user, controller, 2, "Hello");

        Assert.IsType<ForbidResult>(result);
    }

    private sealed class ControllerBaseForTest : ControllerBase
    {
    }
}