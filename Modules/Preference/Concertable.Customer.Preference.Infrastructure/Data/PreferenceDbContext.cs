using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal class PreferenceDbContext(
    DbContextOptions<PreferenceDbContext> options,
    PreferenceConfigurationProvider provider)
    : DbContextBase(options)
{
    public DbSet<PreferenceEntity> Preferences => Set<PreferenceEntity>();
    public DbSet<GenrePreferenceEntity> GenrePreferences => Set<GenrePreferenceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
