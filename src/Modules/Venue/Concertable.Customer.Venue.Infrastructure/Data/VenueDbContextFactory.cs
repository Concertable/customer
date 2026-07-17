using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueDbContextFactory : IDesignTimeDbContextFactory<VenueDbContext>
{
    public VenueDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<VenueDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new VenueDbContext(options, new VenueConfigurationProvider());
    }
}
