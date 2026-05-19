using Concertable.Customer.Review.Infrastructure.Data;

namespace Concertable.Customer.Review.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<ReviewDbContext>;

internal class UnitOfWorkBehavior(IUnitOfWork<ReviewDbContext> unitOfWork)
    : UnitOfWorkBehavior<ReviewDbContext>(unitOfWork), IUnitOfWorkBehavior;
