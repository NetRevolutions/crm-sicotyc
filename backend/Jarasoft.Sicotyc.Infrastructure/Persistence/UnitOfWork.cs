using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence
{
    public sealed class UnitOfWork(
        SicotycDbContext context)
        : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction =
            await context.Database.BeginTransactionAsync(
                cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();

            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();

            _transaction = null;
        }        
    }
}
