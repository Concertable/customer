using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertDbContext(
    DbContextOptions<ConcertDbContext> options,
    ConcertConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    public DbSet<VenueReadModel> VenueReadModels => Set<VenueReadModel>();
    public DbSet<ArtistReadModel> ArtistReadModels => Set<ArtistReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
