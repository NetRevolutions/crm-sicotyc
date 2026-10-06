namespace Jarasoft.Sicotyc.Application.Features.Users
    .Commands.ChangeUserStatus;

public sealed record ChangeUserStatusResponse(
    Guid UserId,
    bool IsActive,
    string Message);