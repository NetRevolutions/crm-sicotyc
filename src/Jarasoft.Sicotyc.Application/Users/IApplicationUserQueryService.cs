namespace Jarasoft.Sicotyc.Application.Users;

public interface IApplicationUserQueryService
{
    Task<ApplicationUserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}