using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Concert.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<ConcertDbContext>;

internal class UnitOfWorkBehavior(IUnitOfWork<ConcertDbContext> unitOfWork)
    : UnitOfWorkBehavior<ConcertDbContext>(unitOfWork), IUnitOfWorkBehavior;
