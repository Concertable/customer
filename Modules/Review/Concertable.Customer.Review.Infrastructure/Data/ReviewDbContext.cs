using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal class ReviewDbContext(
    DbContextOptions<ReviewDbContext> options,
    ReviewConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
