namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserStatus;

public sealed record ChangeUserStatusResult(
    Guid UserId,
    bool IsActive);