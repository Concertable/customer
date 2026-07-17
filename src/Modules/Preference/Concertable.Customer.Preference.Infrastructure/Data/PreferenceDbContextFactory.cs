using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal sealed class PreferenceDbContextFactory : IDesignTimeDbContextFactory<PreferenceDbContext>
{
    public PreferenceDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<PreferenceDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new PreferenceDbContext(options, new PreferenceConfigurationProvider());
    }
}
