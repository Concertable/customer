using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueDbContextFactory : IDesignTimeDbContextFactory<VenueDbContext>
{
    public VenueDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CustomerDb")
            ?? "Server=localhost,1433;Database=concertable-customer;User Id=sa;Password=Password11!;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<VenueDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new VenueDbContext(options, new VenueConfigurationProvider());
    }
}
