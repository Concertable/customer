using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Concertable.Customer.User.Infrastructure.Data;

internal sealed class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConnectionString.Customer();
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlServer(connectionString, o => o.UseNetTopologySuite())
            .Options;
        return new UserDbContext(options, new UserConfigurationProvider());
    }
}
