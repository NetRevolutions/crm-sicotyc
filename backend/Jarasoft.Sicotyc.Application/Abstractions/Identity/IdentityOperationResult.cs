namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public sealed record IdentityOperationResult(
    bool Succeeded,
    IReadOnlyCollection<string> Errors);