using Concertable.Customer.User.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure;
using Concertable.Kernel;

namespace Concertable.Customer.User.Infrastructure.Repositories;

internal abstract class Repository<TEntity>(UserDbContext context)
    : Repository<TEntity, Guid>(context)
    where TEntity : class, IGuidEntity;
