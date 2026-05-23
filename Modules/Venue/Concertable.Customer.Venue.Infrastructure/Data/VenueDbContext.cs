using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal class VenueDbContext : DbContextBase
{
    private readonly VenueConfigurationProvider provider;

    public VenueDbContext(DbContextOptions<VenueDbContext> options, VenueConfigurationProvider provider)
        : base(options)
    {
        this.provider = provider;
    }

    public DbSet<VenueReadModel> Venues => Set<VenueReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
