using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Seeding;
using Concertable.Messaging.Contracts;
using Concertable.Seeding;
using Concertable.Seeding.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data.Seeders;

internal class ConcertDevSeeder : IDevSeeder
{
    public int Order => 8;

    private static readonly Guid PayeeUserId = new("b1000000-0000-0000-0000-000000000001");

    private readonly ConcertDbContext context;
    private readonly IBus bus;
    private readonly TimeProvider timeProvider;

    public ConcertDevSeeder(ConcertDbContext context, IBus bus, TimeProvider timeProvider)
    {
        this.context = context;
        this.bus = bus;
        this.timeProvider = timeProvider;
    }

    public Task MigrateAsync(CancellationToken ct = default) => context.Database.MigrateAsync(ct);

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await context.Concerts.SeedIfEmptyAsync(async () =>
        {
            var now = timeProvider.GetUtcNow().DateTime;
            var period = new DateRange(now.AddDays(5), now.AddDays(5).AddHours(3));
            var concert = ConcertReadModel.Create(
                SeedData.UpcomingConcertId,
                "Upcoming FlatFee Show",
                "An upcoming show available for ticket purchase.",
                null, null,
                150, 20m,
                period,
                now.AddDays(-5),
                1, "Test Artist",
                1, "Test Venue");

            context.Concerts.Add(concert);
            await context.SaveChangesAsync(ct);

            await bus.PublishAsync(new ConcertChangedEvent(
                SeedData.UpcomingConcertId,
                concert.Name,
                concert.About,
                concert.BannerUrl,
                concert.Avatar,
                concert.TotalTickets,
                concert.AvailableTickets,
                concert.Price,
                concert.Period,
                concert.DatePosted,
                1, "Test Artist",
                1, "Test Venue",
                0.0, 0.0,
                [],
                PayeeUserId), ct);
        });
    }
}
