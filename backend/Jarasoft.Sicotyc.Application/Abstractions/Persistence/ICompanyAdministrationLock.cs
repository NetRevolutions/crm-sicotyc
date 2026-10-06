namespace Jarasoft.Sicotyc.Application.Abstractions.Persistence;

public interface ICompanyAdministrationLock
{
    Task AcquireAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync();
}