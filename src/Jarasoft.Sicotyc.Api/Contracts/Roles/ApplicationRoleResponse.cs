namespace Jarasoft.Sicotyc.Api.Contracts.Roles;

public sealed record ApplicationRoleResponse(
    Guid Id,
    string? Name,
    string? NormalizedName);