using Concertable.Customer.Concert.Infrastructure.Data;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal abstract class ReadRepository<TEntity>(ConcertDbContext context)
    : ReadRepository<TEntity, ConcertDbContext, int>(context)
    where TEntity : class, IIdEntity;
