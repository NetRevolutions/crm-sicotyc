
using System.Data;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;

namespace Jarasoft.Sicotyc.Application.Features.Users.Services;

public sealed class AdministratorProtectionService(
    IUnitOfWork unitOfWork,
    ICompanyAdministrationLock companyLock)
{
    public async Task ExecuteAsync(
        Guid companyId,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await unitOfWork.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        try
        {
            await companyLock.AcquireAsync(
                companyId,
                cancellationToken);

            await operation(cancellationToken);

            await unitOfWork.CommitTransactionAsync(
                cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw;
        }
        finally
        {
            await companyLock.ReleaseAsync();
        }
    }
}
