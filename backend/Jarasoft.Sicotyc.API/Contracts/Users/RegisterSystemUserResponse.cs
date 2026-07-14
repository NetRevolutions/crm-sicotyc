namespace Jarasoft.Sicotyc.Api.Contracts.Users;

public sealed record RegisterSystemUserResponse(
    bool Succeeded,
    bool RequiresCompanyContact,
    string Message,
    Guid? UserId,
    Guid? CompanyId,
    Guid? ApplicationRoleId,
    string? ApplicationRoleName,
    RegisterCompanyContactResponse? Contact,
    IReadOnlyDictionary<string, string[]> Errors);

public sealed record RegisterCompanyContactResponse(
    string? FirstName,
    string? LastName,
    string? Email);