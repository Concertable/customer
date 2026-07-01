using Concertable.Customer.Artist.Infrastructure.Data;

namespace Concertable.Customer.Artist.Infrastructure;

internal interface IUnitOfWorkBehavior : IUnitOfWorkBehavior<ArtistDbContext>;

internal sealed class UnitOfWorkBehavior(IUnitOfWork<ArtistDbContext> unitOfWork)
    : UnitOfWorkBehavior<ArtistDbContext>(unitOfWork), IUnitOfWorkBehavior;
