using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Jarasoft.Sicotyc.Infrastructure.Identity;

public sealed class CurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    private ClaimsPrincipal? User =>
        httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid? UserId =>
        TryGetGuidClaim(
            JwtRegisteredClaimNames.Sub);

    public Guid? CompanyId =>
        TryGetGuidClaim("company_id");

    public string? Email =>
        User?.FindFirstValue(
            JwtRegisteredClaimNames.Email);

    public string? FirstName =>
        User?.FindFirstValue(
            JwtRegisteredClaimNames.GivenName);

    public string? LastName =>
        User?.FindFirstValue(
            JwtRegisteredClaimNames.FamilyName);

    public IReadOnlyCollection<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .ToArray()
        ?? Array.Empty<string>();

    public bool IsSuperAdministrator =>
        Roles.Contains(
            ApplicationRoles.SuperAdministrator,
            StringComparer.OrdinalIgnoreCase);

    private Guid? TryGetGuidClaim(
        string claimType)
    {
        var value =
            User?.FindFirstValue(claimType);

        return Guid.TryParse(
            value,
            out var id)
            ? id
            : null;
    }
}