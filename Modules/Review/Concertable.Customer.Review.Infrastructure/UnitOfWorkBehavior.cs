using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Review.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<ReviewDbContext>;

internal class UnitOfWorkBehavior(IUnitOfWork<ReviewDbContext> unitOfWork)
    : UnitOfWorkBehavior<ReviewDbContext>(unitOfWork), IUnitOfWorkBehavior;
