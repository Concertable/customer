using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Profile.Infrastructure.Data;

internal class ProfileDbContext(
    DbContextOptions<ProfileDbContext> options,
    ProfileConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<CustomerProfileEntity> CustomerProfiles => Set<CustomerProfileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
