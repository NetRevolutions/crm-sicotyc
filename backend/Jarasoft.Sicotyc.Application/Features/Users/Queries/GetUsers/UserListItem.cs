namespace Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUsers;

public sealed record UserListItem(
    Guid Id,
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    IReadOnlyCollection<string> Roles);