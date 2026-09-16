using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GaesdeApi;

public static class Utils
{
    public static IActionResult CheckAccessLevel(
        ClaimsPrincipal user,
        ControllerBase controller,
        int requiredAccessLevel,
        string message)
    {
        var accessLevelClaim = user.FindFirst("nivel_acesso")?.Value;

        if (!int.TryParse(accessLevelClaim, out var accessLevel) || accessLevel != requiredAccessLevel)
            return controller.Forbid();

        return controller.Ok(new { message, accessLevel });
    }
}