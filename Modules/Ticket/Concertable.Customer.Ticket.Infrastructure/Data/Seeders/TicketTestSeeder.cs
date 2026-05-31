using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Ticket.Infrastructure.Data.Seeders;

internal class TicketTestSeeder : ITestSeeder
{
    public int Order => 5;

    private readonly TicketDbContext context;
    private readonly SeedState seedData;

    public TicketTestSeeder(TicketDbContext context, SeedState seedData)
    {
        this.context = context;
        this.seedData = seedData;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Tickets.SeedIfEmptyAsync(async () =>
        {
            context.Tickets.AddRange(seedData.Tickets);
            await context.SaveChangesAsync(ct);
        });
    }
}
