using Concertable.Customer.Venue.Infrastructure.Data;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal abstract class ReadRepository<TEntity>(VenueDbContext context)
    : ReadRepository<TEntity, VenueDbContext, int>(context)
    where TEntity : class, IIdEntity;
