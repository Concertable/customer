using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueDbContextFactory : CustomerDesignTimeDbContextFactory<VenueDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override VenueDbContext Create(DbContextOptions<VenueDbContext> options) =>
        new(options, DefaultOutboxOptions, new VenueConfigurationProvider());
}
