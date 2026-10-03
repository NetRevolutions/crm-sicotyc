namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public sealed record IdentityUserListItem(
    Guid Id,
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    bool IsActive,
    IReadOnlyCollection<string> Roles);