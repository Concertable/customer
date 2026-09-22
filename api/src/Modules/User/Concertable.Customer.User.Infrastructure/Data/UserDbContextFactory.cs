using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Concertable.Customer.User.Infrastructure.Data;

internal sealed class UserDbContextFactory : CustomerDesignTimeDbContextFactory<UserDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override UserDbContext Create(DbContextOptions<UserDbContext> options) =>
        new(options, DefaultOutboxOptions, new UserConfigurationProvider());

    protected override void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql) =>
        npgsql.UseNetTopologySuite();
}
