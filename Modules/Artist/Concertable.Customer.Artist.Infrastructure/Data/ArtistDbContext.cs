using Concertable.Customer.Artist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal class ArtistDbContext : DbContextBase
{
    private readonly ArtistConfigurationProvider provider;

    public ArtistDbContext(DbContextOptions<ArtistDbContext> options, ArtistConfigurationProvider provider)
        : base(options)
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
