namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;

public sealed record ChangeUserRoleCommand(
    Guid UserId,
    string Role);