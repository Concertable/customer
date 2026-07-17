using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertDbContextFactory : CustomerDesignTimeDbContextFactory<ConcertDbContext>
{
    protected override ConcertDbContext Create(DbContextOptions<ConcertDbContext> options) =>
        new(options, new ConcertConfigurationProvider());
}
