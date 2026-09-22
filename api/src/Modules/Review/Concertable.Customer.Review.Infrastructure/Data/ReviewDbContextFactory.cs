using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewDbContextFactory : CustomerDesignTimeDbContextFactory<ReviewDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override ReviewDbContext Create(DbContextOptions<ReviewDbContext> options) =>
        new(options, DefaultOutboxOptions, new ReviewConfigurationProvider());
}
