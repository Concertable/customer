using Concertable.Contracts;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure;
using Concertable.DataAccess.Infrastructure.Specifications;
using Concertable.Kernel;
using Concertable.Kernel.Specifications;
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

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await query.ToListAsync(ct);

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        query.FirstOrDefaultAsync(entity => entity.Id!.Equals(id), ct);

    public Task<TEntity?> GetByIdAsync(TKey id, ISpecification<TEntity> spec, CancellationToken ct = default) =>
        query.Apply(spec).FirstOrDefaultAsync(entity => entity.Id!.Equals(id), ct);

    public Task<TResult?> GetByIdAsync<TResult>(TKey id, ISpecification<TEntity, TResult> spec, CancellationToken ct = default)
        where TResult : class =>
        query.Where(entity => entity.Id!.Equals(id)).Select(spec.Selector).FirstOrDefaultAsync(ct);

    public Task<TResult?> GetByIdAsync<TResult>(TKey id, ISpecification<TEntity, TResult?> spec, CancellationToken ct = default)
        where TResult : struct =>
        query.Where(entity => entity.Id!.Equals(id)).Select(spec.Selector).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(ISpecification<TEntity> spec, CancellationToken ct = default) =>
        await query.Apply(spec).ApplyOrders(spec).ToListAsync(ct);

    public async Task<IReadOnlyList<TResult>> GetAllAsync<TResult>(ISpecification<TEntity, TResult> spec, CancellationToken ct = default) =>
        await query.ApplyOrders(spec).Select(spec.Selector).ToListAsync(ct);

    public Task<IPagination<TEntity>> GetPageAsync(ISpecification<TEntity> spec, IPageParams pageParams, CancellationToken ct = default) =>
        query.Apply(spec).ApplyOrders(spec).ToPaginationAsync(pageParams, ct);

    public Task<IPagination<TResult>> GetPageAsync<TResult>(ISpecification<TEntity, TResult> spec, IPageParams pageParams, CancellationToken ct = default) =>
        query.ApplyOrders(spec).Select(spec.Selector).ToPaginationAsync(pageParams, ct);

    public bool Exists(TKey id) => query.Any(entity => entity.Id!.Equals(id));
}
