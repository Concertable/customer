using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal sealed class ArtistDbContextFactory : IDesignTimeDbContextFactory<ArtistDbContext>
{
    public ArtistDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<ArtistDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ArtistDbContext(options, new ArtistConfigurationProvider());
    }
}
