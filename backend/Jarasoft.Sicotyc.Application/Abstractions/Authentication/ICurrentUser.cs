public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? CompanyId { get; }

    string? Email { get; }

    string? FirstName { get; }

    string? LastName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsSuperAdministrator { get; }
}