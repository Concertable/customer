using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Kernel;

namespace Concertable.Customer.Ticket.Infrastructure.Repositories;

internal abstract class WriteRepository<TEntity>(TicketDbContext context)
    : WriteRepository<TEntity, TicketDbContext>(context)
    where TEntity : class;
