namespace Jarasoft.Sicotyc.Application.Users;

public sealed record RegisterSystemUserCommand(
    string FirstName,
    string LastName,
    string DocumentType,
    string DocumentNumber,
    string UserName,
    string Email,
    string PhoneNumber,
    Guid ApplicationRoleId,
    string Password,
    string Ruc,
    string BusinessName,
    string Address,
    string CompanyEmail,
    string CompanyPhone,
    bool IsTransportCompany);