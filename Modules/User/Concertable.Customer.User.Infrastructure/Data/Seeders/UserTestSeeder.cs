using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.User.Infrastructure.Data.Seeders;

internal class UserTestSeeder : ITestSeeder
{
    public int Order => 0;

    private readonly UserDbContext context;
    private readonly SeedState seedData;

    public UserTestSeeder(UserDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Users.SeedIfEmptyAsync(async () =>
        {
            context.Users.AddRange(seedData.Customers);
            await context.SaveChangesAsync(ct);
        });
    }
}
