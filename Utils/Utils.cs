using System.Security.Claims;
using GaesdeApi.DTOs;
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

    public static PaginatedResponse<T> Paginate<T>(IEnumerable<T> items, PaginationRequest request)
    {
        var page = request.ValidPage;
        var pageSize = request.ValidPageSize;
        var source = items?.ToArray() ?? Array.Empty<T>();
        var totalItems = source.Length;
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var pageItems = source.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return new PaginatedResponse<T>(pageItems, page, pageSize, totalItems, totalPages);
    }
}