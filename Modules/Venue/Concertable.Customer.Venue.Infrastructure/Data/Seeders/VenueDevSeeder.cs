using Concertable.Customer.Seeding;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data.Seeders;

internal class VenueDevSeeder : IDevSeeder
{
    public int Order => 3;

    private readonly VenueDbContext context;
    private readonly SeedData seedData;

    public VenueDevSeeder(VenueDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Venues.SeedIfEmptyAsync(async () =>
        {
            context.Venues.Add(seedData.Venue);
            await context.SaveChangesAsync(ct);
        });
    }
}
