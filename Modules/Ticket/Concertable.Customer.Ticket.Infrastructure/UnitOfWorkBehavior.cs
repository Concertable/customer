using Concertable.Customer.Ticket.Infrastructure.Data;

namespace Concertable.Customer.Ticket.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<TicketDbContext>;

internal class UnitOfWorkBehavior(IUnitOfWork<TicketDbContext> unitOfWork)
    : UnitOfWorkBehavior<TicketDbContext>(unitOfWork), IUnitOfWorkBehavior;
