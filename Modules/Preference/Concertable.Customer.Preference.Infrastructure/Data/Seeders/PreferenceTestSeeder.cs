using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Concertable.Customer.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data.Seeders;

internal class PreferenceTestSeeder : ITestSeeder
{
    public int Order => 7;

    private readonly PreferenceDbContext context;
    private readonly SeedData seedData;

    public PreferenceTestSeeder(PreferenceDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Preferences.SeedIfEmptyAsync(async () =>
        {
            context.Preferences.Add(
                PreferenceEntity.Create(seedData.Customer.Id, 25, [Genre.Rock]));

            await context.SaveChangesAsync(ct);
        });
    }
}
