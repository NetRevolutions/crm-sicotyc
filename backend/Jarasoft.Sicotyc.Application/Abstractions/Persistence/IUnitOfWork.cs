using System.Data;

namespace Jarasoft.Sicotyc.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default);
}