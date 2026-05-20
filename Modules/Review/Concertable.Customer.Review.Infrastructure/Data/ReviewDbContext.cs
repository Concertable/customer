using Concertable.Messaging.Domain;
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
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);

        modelBuilder.Entity<OutboxMessageEntity>(b =>
        {
            b.ToTable("Outbox", "messaging", t => t.ExcludeFromMigrations());
            b.Property(m => m.Id).ValueGeneratedNever();
        });
    }
}
