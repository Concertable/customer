using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Data.Seeders;

internal sealed class ArtistProjectionTestSeeder : ITestSeeder
{
    public int Order => 2;

    private readonly ArtistDbContext context;
    private readonly SeedState seedData;

    public ArtistProjectionTestSeeder(ArtistDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Artists.SeedIfEmptyAsync(async () =>
        {
            context.Artists.AddRange(seedData.Artists);
            await context.SaveChangesAsync(ct);
        });
    }
}
