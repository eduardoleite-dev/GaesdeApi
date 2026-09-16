using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi.Tests;

internal static class ControllerTestHelpers
{
    public static void SetUser(ControllerBase controller, string? userId = null, string? role = null)
    {
        var claims = new List<Claim>();
        if (userId is not null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        if (role is not null)
            claims.Add(new Claim(ClaimTypes.Role, role));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }
}