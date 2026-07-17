using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewDbContextFactory : IDesignTimeDbContextFactory<ReviewDbContext>
{
    public ReviewDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<ReviewDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ReviewDbContext(options, new ReviewConfigurationProvider());
    }
}
