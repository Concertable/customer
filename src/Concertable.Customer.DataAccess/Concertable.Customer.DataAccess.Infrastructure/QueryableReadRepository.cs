using Concertable.DataAccess.Application;
using Concertable.Kernel;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.DataAccess.Infrastructure;

public abstract class QueryableReadRepository<TEntity, TKey> : IReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly IQueryable<TEntity> query;

    protected QueryableReadRepository(IQueryable<TEntity> query)
    {
        this.query = query;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await query.ToListAsync(ct);

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        query.FirstOrDefaultAsync(entity => entity.Id!.Equals(id), ct);

    public bool Exists(TKey id) => query.Any(entity => entity.Id!.Equals(id));
}
