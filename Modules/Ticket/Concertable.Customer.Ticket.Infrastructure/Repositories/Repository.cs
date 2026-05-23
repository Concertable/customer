using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Kernel;

namespace Concertable.Customer.Ticket.Infrastructure.Repositories;

internal abstract class BaseRepository<TEntity>(TicketDbContext context)
    : BaseRepository<TEntity, TicketDbContext>(context)
    where TEntity : class;

internal abstract class Repository<TEntity>(TicketDbContext context)
    : Repository<TEntity, TicketDbContext>(context)
    where TEntity : class, IIdEntity;

internal abstract class GuidRepository<TEntity>(TicketDbContext context)
    : GuidRepository<TEntity, TicketDbContext>(context)
    where TEntity : class, IGuidEntity;
