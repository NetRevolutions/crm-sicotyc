using System.Linq.Expressions;

namespace Jarasoft.Sicotyc.Application.Interfaces.Repositories;

public interface IReadRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> FindAsync(params object[] keyValues);

    Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    IQueryable<TEntity> Query();
}

public interface IRepository<TEntity> : IReadRepository<TEntity>
    where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
