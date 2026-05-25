using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal class ConcertDbContextFactory : IDesignTimeDbContextFactory<ConcertDbContext>
{
    public ConcertDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CustomerDb")
            ?? "Server=localhost,1433;Database=concertable-customer;User Id=sa;Password=Password11!;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<ConcertDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ConcertDbContext(options, new ConcertConfigurationProvider());
    }
}
