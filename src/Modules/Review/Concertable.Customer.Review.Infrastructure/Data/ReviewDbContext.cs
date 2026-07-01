using Concertable.Customer.Review.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewDbContext(
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
