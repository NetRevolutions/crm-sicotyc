namespace Jarasoft.Sicotyc.Application.Roles;

public interface IApplicationRoleQueryService
{
    Task<IReadOnlyList<ApplicationRoleDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<ApplicationRoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}