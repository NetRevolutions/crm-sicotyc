namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserResult(
    Guid UserId,
    Guid CompanyId,
    string Email,
    string Role);