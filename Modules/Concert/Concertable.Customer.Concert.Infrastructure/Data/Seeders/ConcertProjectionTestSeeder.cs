using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data.Seeders;

internal sealed class ConcertProjectionTestSeeder : ITestSeeder
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
        await context.VenueReadModels.SeedIfEmptyAsync(async () =>
        {
            context.VenueReadModels.AddRange(seedData.Venues.Select(v => new VenueReadModel
            {
                Id = v.Id,
                Name = v.Name,
                Address = v.Address,
                Latitude = v.Latitude,
                Longitude = v.Longitude
            }));
            await context.SaveChangesAsync(ct);
        });

        await context.ArtistReadModels.SeedIfEmptyAsync(async () =>
        {
            context.ArtistReadModels.AddRange(seedData.Artists.Select(a => new ArtistReadModel
            {
                Id = a.Id,
                Name = a.Name,
                Avatar = a.Avatar,
                AverageRating = a.AverageRating,
                ReviewCount = a.ReviewCount,
                Address = a.Address,
                Genres = a.Genres.Select(g => new ArtistReadModelGenre { ArtistReadModelId = a.Id, Genre = g.Genre }).ToList()
            }));
            await context.SaveChangesAsync(ct);
        });

        await context.Concerts.SeedIfEmptyAsync(async () =>
        {
            context.Concerts.AddRange(seedData.Concerts);
            await context.SaveChangesAsync(ct);
        });
    }
}
