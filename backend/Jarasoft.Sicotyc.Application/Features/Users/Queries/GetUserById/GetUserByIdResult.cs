namespace Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdResult(
    Guid Id,
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    IReadOnlyCollection<string> Roles);