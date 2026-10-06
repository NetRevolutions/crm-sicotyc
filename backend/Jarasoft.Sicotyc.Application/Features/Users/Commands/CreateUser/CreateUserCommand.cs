namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? CompanyId);