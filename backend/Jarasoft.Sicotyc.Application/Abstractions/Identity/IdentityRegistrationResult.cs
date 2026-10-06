namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public sealed record IdentityRegistrationResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyCollection<string> Errors);