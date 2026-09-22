using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueDbContext : DbContextBase
{
    private readonly VenueConfigurationProvider provider;

    public VenueDbContext(
        DbContextOptions<VenueDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        VenueConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

    public DbSet<VenueEntity> Venues => Set<VenueEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
