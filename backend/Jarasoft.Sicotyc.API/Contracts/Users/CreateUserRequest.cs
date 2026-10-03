namespace Jarasoft.Sicotyc.API.Contracts.Users;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? CompanyId);