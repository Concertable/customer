using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Contracts.Enums;
using Concertable.Kernel.ValueObjects;

namespace Concertable.Customer.Concert.IntegrationTests;

/// <summary>Builds the <see cref="ConcertChangedEvent"/> the projection tests dispatch to create a concert.</summary>
internal static class ConcertChangedEvents
{
    public static ConcertChangedEvent Create(
        int concertId,
        string name = "Concert",
        int totalTickets = 10,
        IReadOnlyCollection<Genre>? genres = null) =>
        new(
            concertId, name, "About", "avatar.png", "banner.png",
            totalTickets, totalTickets, 25m,
            new DateRange(TestTime.Now.UtcDateTime.AddDays(30), TestTime.Now.UtcDateTime.AddDays(31)),
            TestTime.Now.UtcDateTime,
            5, "Artist", 7, "Venue", 51.5, -0.1,
            genres ?? [Genre.Rock],
            Guid.NewGuid(), Guid.NewGuid());
}
