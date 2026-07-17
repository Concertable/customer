using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Concertable.Customer.User.Infrastructure.Data;

internal sealed class UserDbContextFactory : CustomerDesignTimeDbContextFactory<UserDbContext>
{
    protected override UserDbContext Create(DbContextOptions<UserDbContext> options) =>
        new(options, new UserConfigurationProvider());

    protected override void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql) =>
        sql.UseNetTopologySuite();
}
