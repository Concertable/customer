using Concertable.Customer.Venue.Infrastructure.Data;

namespace Concertable.Customer.Venue.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<VenueDbContext>;

internal class UnitOfWorkBehavior(IUnitOfWork<VenueDbContext> unitOfWork)
    : UnitOfWorkBehavior<VenueDbContext>(unitOfWork), IUnitOfWorkBehavior;
