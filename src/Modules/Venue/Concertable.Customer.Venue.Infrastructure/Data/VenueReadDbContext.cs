using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueReadDbContext(
    DbContextOptions<VenueReadDbContext> options,
    VenueConfigurationProvider provider)
    : ReadDbContext(options, provider, Schema.Name)
{
    public IQueryable<VenueEntity> Venues => Set<VenueEntity>();
}
