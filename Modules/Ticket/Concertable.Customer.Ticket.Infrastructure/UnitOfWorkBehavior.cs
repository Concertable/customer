using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Ticket.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<TicketDbContext>;

internal class UnitOfWorkBehavior(IUnitOfWork<TicketDbContext> unitOfWork)
    : UnitOfWorkBehavior<TicketDbContext>(unitOfWork), IUnitOfWorkBehavior;
