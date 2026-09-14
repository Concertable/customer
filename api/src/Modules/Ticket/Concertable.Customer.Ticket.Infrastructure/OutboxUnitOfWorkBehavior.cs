using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure;
using Concertable.Messaging.Infrastructure.Outbox;

namespace Concertable.Customer.Ticket.Infrastructure;

internal interface IOutboxUnitOfWorkBehavior : IOutboxUnitOfWorkBehavior<TicketDbContext>;

internal sealed class OutboxUnitOfWorkBehavior(TicketDbContext context, IDbContextAccessor accessor)
    : OutboxUnitOfWorkBehavior<TicketDbContext>(context, accessor), IOutboxUnitOfWorkBehavior;
