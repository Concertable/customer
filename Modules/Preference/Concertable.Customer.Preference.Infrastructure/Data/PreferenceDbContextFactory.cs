using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal sealed class PreferenceDbContextFactory : IDesignTimeDbContextFactory<PreferenceDbContext>
{
    public PreferenceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CustomerDb")
            ?? "Server=localhost,1433;Database=concertable-customer;User Id=sa;Password=Password11!;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<PreferenceDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new PreferenceDbContext(options, new PreferenceConfigurationProvider());
    }
}
