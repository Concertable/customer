using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data.Seeders;

internal class ConcertProjectionTestSeeder : ITestSeeder
{
    public int Order => 3;

    private readonly ConcertDbContext context;
    private readonly SeedState seedData;

    public ConcertProjectionTestSeeder(ConcertDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Concerts.SeedIfEmptyAsync(async () =>
        {
            context.Concerts.AddRange(seedData.Concerts);
            await context.SaveChangesAsync(ct);
        });
    }
}
