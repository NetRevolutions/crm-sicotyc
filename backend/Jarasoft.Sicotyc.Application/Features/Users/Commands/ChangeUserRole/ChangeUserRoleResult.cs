namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;

public sealed record ChangeUserRoleResult(
    Guid UserId,
    string Role);