using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal sealed class ArtistDbContext : DbContextBase
{
    private readonly ArtistConfigurationProvider provider;

    public ArtistDbContext(
        DbContextOptions<ArtistDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        ArtistConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
