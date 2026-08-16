using Concertable.Customer.Venue.Domain.Entities;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueReadDbContext(
    DbContextOptions<VenueReadDbContext> options,
    VenueConfigurationProvider provider)
    : ReadDbContext(options, provider, Schema.Name), IVenueReadDbContext
{
    IQueryable<VenueEntity> IVenueReadDbContext.Venues => Query<VenueEntity>();
}
