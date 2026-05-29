using Concertable.Customer.Seeding;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Data.Seeders;

internal class ArtistDevSeeder : IDevSeeder
{
    public int Order => 4;

    private readonly ArtistDbContext context;
    private readonly SeedData seedData;

    public ArtistDevSeeder(ArtistDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            context.Artists.Add(seedData.Artist);
            await context.SaveChangesAsync(ct);
        });
    }
}
