using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data.Seeders;

internal sealed class VenueProjectionTestSeeder : ITestSeeder
{
    public int Order => 1;

    private readonly VenueDbContext context;
    private readonly SeedState seedData;

    public VenueProjectionTestSeeder(VenueDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Venues.SeedIfEmptyAsync(async () =>
        {
            context.Venues.AddRange(seedData.Venues);
            await context.SaveChangesAsync(ct);
        });
    }
}
