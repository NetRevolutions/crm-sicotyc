namespace Jarasoft.Sicotyc.Application.Users;

public sealed record ApplicationUserDto(
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