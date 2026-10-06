namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public sealed record IdentityBulkOperationResult(
    bool Succeeded,
    int AffectedUsers,
    IReadOnlyCollection<string> Errors);