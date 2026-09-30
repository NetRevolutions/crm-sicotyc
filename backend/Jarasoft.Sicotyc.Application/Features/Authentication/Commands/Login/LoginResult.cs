namespace Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;

public sealed record LoginResult(
    Guid UserId,
    Guid CompanyId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles,
    string AccessToken,
    DateTime ExpiresAt);