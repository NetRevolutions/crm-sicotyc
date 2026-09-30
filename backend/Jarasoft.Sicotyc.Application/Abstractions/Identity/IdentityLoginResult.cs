namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public sealed record IdentityLoginResult(
    bool Succeeded,
    Guid? UserId,
    Guid? CompanyId,
    string? Email,
    string? FirstName,
    string? LastName,
    IReadOnlyCollection<string> Roles);