using System.Linq.Expressions;
using Jarasoft.Sicotyc.Application.Interfaces.Repositories;
using Jarasoft.Sicotyc.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infraestructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> Entities;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        Entities = context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
    }

    public Task<TEntity?> FindAsync(params object[] keyValues)
    {
        return Entities.FindAsync(keyValues).AsTask();
    }

    public Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Entities.ToListAsync(cancellationToken);
    }

    public Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Entities.Where(predicate).ToListAsync(cancellationToken);
    }

    public IQueryable<TEntity> Query()
    {
        return Entities.AsQueryable();
    }

    public void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Entities.Update(entity);
    }
}
