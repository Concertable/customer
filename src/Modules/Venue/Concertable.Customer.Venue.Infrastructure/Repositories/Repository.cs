using Concertable.Customer.Venue.Infrastructure.Data;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal abstract class ReadRepository<TEntity>(VenueReadDbContext context)
    : ReadRepository<TEntity, VenueReadDbContext, int>(context)
    where TEntity : class, IIdEntity;
