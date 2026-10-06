namespace Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;

public sealed record DeactivateCompanyUsersResult(
    Guid CompanyId,
    int DeactivatedUsers);