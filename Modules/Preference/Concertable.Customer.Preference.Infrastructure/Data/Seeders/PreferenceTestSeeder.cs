using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data.Seeders;

internal sealed class PreferenceTestSeeder : ITestSeeder
{
    public int Order => 7;

    private readonly PreferenceDbContext context;
    private readonly SeedState seedData;

    public PreferenceTestSeeder(PreferenceDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Preferences.SeedIfEmptyAsync(async () =>
        {
            context.Preferences.AddRange(seedData.Preferences);
            await context.SaveChangesAsync(ct);
        });
    }
}
