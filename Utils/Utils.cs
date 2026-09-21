using System.Security.Claims;
using GaesdeApi.DTOs;
using GaesdeApi.Models;
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
        var accessLevel = GetAccessLevel(user);

        if (accessLevel != (AccessLevel)requiredAccessLevel)
            return controller.Forbid();

        return controller.Ok(new { message, accessLevel = (int)accessLevel });
    }

    public static AccessLevel GetAccessLevel(ClaimsPrincipal user)
    {
        var accessLevelClaim = user.FindFirst("nivel_acesso")?.Value;
        if (int.TryParse(accessLevelClaim, out var parsedLevel) && Enum.IsDefined(typeof(AccessLevel), parsedLevel))
            return (AccessLevel)parsedLevel;

        if (HasRoleClaim(user, nameof(AccessLevel.Administrador)))
            return AccessLevel.Administrador;
        if (HasRoleClaim(user, nameof(AccessLevel.Professor)))
            return AccessLevel.Professor;
        if (HasRoleClaim(user, nameof(AccessLevel.Vendedor)))
            return AccessLevel.Vendedor;
        if (HasRoleClaim(user, nameof(AccessLevel.Aluno)))
            return AccessLevel.Aluno;

        return AccessLevel.Aluno;
    }

    public static bool IsStudent(ClaimsPrincipal user) => GetAccessLevel(user) == AccessLevel.Aluno;

    public static bool IsAdministrator(ClaimsPrincipal user) => GetAccessLevel(user) == AccessLevel.Administrador;

    public static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("nameid")
            ?? user.FindFirstValue("sub");
    }

    public static bool HasRoleClaim(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role)
            || user.HasClaim(claim =>
                (claim.Type == ClaimTypes.Role || claim.Type == "role" || claim.Type == "roles") &&
                string.Equals(claim.Value, role, StringComparison.OrdinalIgnoreCase));
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