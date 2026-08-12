using Concertable.Customer.Concert.Infrastructure.Data;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal abstract class ReadRepository<TEntity>(ConcertReadDbContext context)
    : ReadRepository<TEntity, ConcertReadDbContext, int>(context)
    where TEntity : class, IIdEntity;
