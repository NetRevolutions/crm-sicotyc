using Jarasoft.Sicotyc.Application.Users;
using Jarasoft.Sicotyc.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infraestructure.Services;

public sealed class ApplicationUserQueryService(ApplicationDbContext context) : IApplicationUserQueryService
{
    public async Task<ApplicationUserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new ApplicationUserDto(
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.FirstName != null ? user.FirstName.Value : null,
                user.LastName != null ? user.LastName.Value : null,
                user.DocumentIdentity != null ? user.DocumentIdentity.DocumentType.ToString() : null,
                user.DocumentIdentity != null ? user.DocumentIdentity.DocumentNumber : null,
                user.ApplicationRoleId,
                user.ApplicationRole != null ? user.ApplicationRole.Name : null,
                user.UserCompanies.Select(company => company.CompanyId).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}