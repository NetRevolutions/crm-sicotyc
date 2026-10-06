namespace Jarasoft.Sicotyc.Application.Common.DTOs;

public sealed record IdentityUserDto
{
    public Guid Id { get; init; }

    public Guid CompanyId { get; init; }

    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; }
        = Array.Empty<string>();
}