using Concertable.DataAccess.Application;
using Concertable.Kernel;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.DataAccess.Infrastructure;

public abstract class QueryableReadRepository<TEntity, TKey>(IQueryable<TEntity> query)
    : IReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await query.ToListAsync(ct);

    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        query.FirstOrDefaultAsync(entity => entity.Id!.Equals(id), ct);

    public bool Exists(TKey id) => query.Any(entity => entity.Id!.Equals(id));
}
