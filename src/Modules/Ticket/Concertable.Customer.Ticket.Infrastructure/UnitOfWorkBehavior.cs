using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Ticket.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<TicketDbContext>;

internal sealed class UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    : UnitOfWorkBehavior<TicketDbContext>(unitOfWork), IUnitOfWorkBehavior;
