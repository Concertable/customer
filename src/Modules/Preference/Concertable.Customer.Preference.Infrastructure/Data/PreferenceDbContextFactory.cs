using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal sealed class PreferenceDbContextFactory : CustomerDesignTimeDbContextFactory<PreferenceDbContext>
{
    protected override PreferenceDbContext Create(DbContextOptions<PreferenceDbContext> options) =>
        new(options, new PreferenceConfigurationProvider());
}
