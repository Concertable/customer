using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertDbContextFactory : IDesignTimeDbContextFactory<ConcertDbContext>
{
    public ConcertDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<ConcertDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ConcertDbContext(options, new ConcertConfigurationProvider());
    }
}
