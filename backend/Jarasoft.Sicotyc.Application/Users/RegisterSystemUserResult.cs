namespace Jarasoft.Sicotyc.Application.Users;

public sealed record RegistrationContactInfo(
    string? FirstName,
    string? LastName,
    string? Email);

public sealed record RegisterSystemUserResult(
    bool Succeeded,
    bool RequiresCompanyContact,
    string Message,
    Guid? UserId,
    Guid? CompanyId,
    Guid? ApplicationRoleId,
    string? ApplicationRoleName,
    RegistrationContactInfo? Contact,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static RegisterSystemUserResult Success(
        string message,
        Guid userId,
        Guid companyId,
        Guid? applicationRoleId,
        string? applicationRoleName) =>
        new(
            true,
            false,
            message,
            userId,
            companyId,
            applicationRoleId,
            applicationRoleName,
            null,
            new Dictionary<string, string[]>());

    public static RegisterSystemUserResult ContactRequired(string message, RegistrationContactInfo? contact) =>
        new(
            false,
            true,
            message,
            null,
            null,
            null,
            null,
            contact,
            new Dictionary<string, string[]>());

    public static RegisterSystemUserResult Failure(string message, IDictionary<string, string[]>? errors = null) =>
        new(
            false,
            false,
            message,
            null,
            null,
            null,
            null,
            null,
            errors is null
                ? new Dictionary<string, string[]>()
                : new Dictionary<string, string[]>(errors));
}