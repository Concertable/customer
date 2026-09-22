using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal sealed class PreferenceDbContextFactory : CustomerDesignTimeDbContextFactory<PreferenceDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override PreferenceDbContext Create(DbContextOptions<PreferenceDbContext> options) =>
        new(options, DefaultOutboxOptions, new PreferenceConfigurationProvider());
}
