using Concertable.Customer.Review.Infrastructure.Data;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal abstract class WriteRepository<TEntity>(ReviewDbContext context)
    : Concertable.DataAccess.Infrastructure.WriteRepository<TEntity>(context)
    where TEntity : class;
