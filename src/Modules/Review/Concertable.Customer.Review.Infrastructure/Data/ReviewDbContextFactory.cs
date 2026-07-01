using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewDbContextFactory : IDesignTimeDbContextFactory<ReviewDbContext>
{
    public ReviewDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CustomerDb")
            ?? "Server=localhost,1433;Database=concertable-customer;User Id=sa;Password=Password11!;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<ReviewDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ReviewDbContext(options, new ReviewConfigurationProvider());
    }
}
