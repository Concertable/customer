using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Ticket.Infrastructure;

internal interface IUnitOfWork : IUnitOfWork<TicketDbContext>;

internal class UnitOfWork(TicketDbContext context)
    : Concertable.DataAccess.Infrastructure.UnitOfWork<TicketDbContext>(context), IUnitOfWork;
