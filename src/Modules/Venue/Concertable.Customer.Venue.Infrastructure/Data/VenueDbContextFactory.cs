using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueDbContextFactory : CustomerDesignTimeDbContextFactory<VenueDbContext>
{
    protected override VenueDbContext Create(DbContextOptions<VenueDbContext> options) =>
        new(options, new VenueConfigurationProvider());
}
