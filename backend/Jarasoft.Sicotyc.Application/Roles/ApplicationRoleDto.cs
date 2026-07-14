namespace Jarasoft.Sicotyc.Application.Roles;

public sealed record ApplicationRoleDto(
    Guid Id,
    string? Name,
    string? NormalizedName);