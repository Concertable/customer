using Concertable.Customer.Seeding;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data.Seeders;

internal class ConcertDevSeeder : IDevSeeder
{
    public int Order => 5;

    private readonly ConcertDbContext context;
    private readonly SeedData seedData;

    public ConcertDevSeeder(ConcertDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Concerts.SeedIfEmptyAsync(async () =>
        {
            context.Concerts.Add(seedData.UpcomingConcert);
            await context.SaveChangesAsync(ct);
        });
    }
}
