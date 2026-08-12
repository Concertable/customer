using Concertable.DataAccess.Application;
using Concertable.Kernel;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.DataAccess.Infrastructure;

public abstract class ReadRepository<TEntity>(IReadDbContext context) : IReadRepository<TEntity>
    where TEntity : class, IIdEntity
{
    protected readonly IReadDbContext context = context;

    public virtual Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        context.Query<TEntity>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await context.Query<TEntity>().ToListAsync(ct);

    public bool Exists(int id) =>
        context.Query<TEntity>().Any(e => e.Id == id);
}
