using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal class ConcertDbContext(
    DbContextOptions<ConcertDbContext> options,
    ConcertConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
