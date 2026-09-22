using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertDbContext : DbContextBase
{
    private readonly ConcertConfigurationProvider provider;

    public ConcertDbContext(
        DbContextOptions<ConcertDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        ConcertConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

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
