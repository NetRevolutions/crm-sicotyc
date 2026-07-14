namespace Jarasoft.Sicotyc.Api.Contracts.Users;

public sealed record ApplicationUserResponse(
    Guid Id,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? FirstName,
    string? LastName,
    string? DocumentType,
    string? DocumentNumber,
    Guid? ApplicationRoleId,
    string? ApplicationRoleName,
    IReadOnlyList<Guid> CompanyIds);