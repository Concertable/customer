using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertDbContextFactory : CustomerDesignTimeDbContextFactory<ConcertDbContext>
{
    protected override string MigrationsSchema => Schema.Name;

    protected override ConcertDbContext Create(DbContextOptions<ConcertDbContext> options) =>
        new(options, DefaultOutboxOptions, new ConcertConfigurationProvider());
}
