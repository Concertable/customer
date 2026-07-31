using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewDbContextFactory : CustomerDesignTimeDbContextFactory<ReviewDbContext>
{
    protected override ReviewDbContext Create(DbContextOptions<ReviewDbContext> options) =>
        new(options, new ReviewConfigurationProvider());
}
