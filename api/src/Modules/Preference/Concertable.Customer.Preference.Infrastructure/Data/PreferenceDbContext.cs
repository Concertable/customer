using Concertable.Messaging.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal sealed class PreferenceDbContext : DbContextBase
{
    private readonly PreferenceConfigurationProvider provider;

    public PreferenceDbContext(
        DbContextOptions<PreferenceDbContext> options,
        IOptions<OutboxOptions> outboxOptions,
        PreferenceConfigurationProvider provider)
        : base(options, outboxOptions)
    {
        this.provider = provider;
    }

    public DbSet<PreferenceEntity> Preferences => Set<PreferenceEntity>();
    public DbSet<GenrePreferenceEntity> GenrePreferences => Set<GenrePreferenceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema.Name);
        provider.Configure(modelBuilder);
    }
}
