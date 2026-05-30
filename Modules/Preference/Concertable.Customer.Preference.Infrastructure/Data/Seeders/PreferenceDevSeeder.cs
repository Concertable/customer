using Concertable.Seed;
using Concertable.Seed.Extensions;
using Concertable.Customer.Seed;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data.Seeders;

internal class PreferenceDevSeeder : IDevSeeder
{
    public int Order => 7;

    private readonly PreferenceDbContext context;
    private readonly SeedData seedData;

    public PreferenceDevSeeder(PreferenceDbContext context, SeedData seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Preferences.SeedIfEmptyAsync(async () =>
        {
            var customerIds = seedData.CustomerIds;
            if (customerIds.Count < 3)
                return;

            context.Preferences.AddRange(
                PreferenceEntity.Create(customerIds[0], 10, [Genre.Rock]),
                PreferenceEntity.Create(customerIds[1], 25, []),
                PreferenceEntity.Create(customerIds[2], 50, []));

            await context.SaveChangesAsync(ct);
        });
    }
}
