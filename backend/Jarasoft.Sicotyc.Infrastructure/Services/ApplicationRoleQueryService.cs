using Jarasoft.Sicotyc.Application.Roles;
using Jarasoft.Sicotyc.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infraestructure.Services;

public sealed class ApplicationRoleQueryService(ApplicationDbContext context) : IApplicationRoleQueryService
{
    public async Task<IReadOnlyList<ApplicationRoleDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new ApplicationRoleDto(role.Id, role.Name, role.NormalizedName))
            .ToListAsync(cancellationToken);
    }

    public Task<ApplicationRoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Roles
            .AsNoTracking()
            .Where(role => role.Id == id)
            .Select(role => new ApplicationRoleDto(role.Id, role.Name, role.NormalizedName))
            .FirstOrDefaultAsync(cancellationToken);
    }
}