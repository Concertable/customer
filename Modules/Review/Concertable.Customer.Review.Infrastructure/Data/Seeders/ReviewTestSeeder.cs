using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Data.Seeders;

internal class ReviewTestSeeder : ITestSeeder
{
    public int Order => 6;

    private readonly ReviewDbContext context;
    private readonly SeedState seedData;

    public ReviewTestSeeder(ReviewDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Reviews.SeedIfEmptyAsync(async () =>
        {
            context.Reviews.AddRange(seedData.Reviews);
            await context.SaveChangesAsync(ct);
        });
    }
}
