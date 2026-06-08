using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.UnitTests.Handlers;

public sealed class TicketPurchasedHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PayeeUserId = Guid.NewGuid();

    private static ConcertDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<ConcertDbContext>().UseInMemoryDatabase(dbName).Options,
            new ConcertConfigurationProvider());

    private static ConcertEntity NewConcert(int concertId = 1, int totalTickets = 10) =>
        ConcertEntity.Create(
            concertId, "Concert", "About", "banner.png", "avatar.png",
            totalTickets, 25m,
            new DateRange(Base.UtcDateTime.AddDays(30), Base.UtcDateTime.AddDays(31)),
            Base.UtcDateTime,
            5, "Artist", 7, "Venue", PayeeUserId);

    private static TicketPurchasedEvent NewEvent(int concertId = 1) =>
        new(Guid.NewGuid(), Guid.NewGuid(), concertId, 25m, Base.UtcDateTime);

    [Fact]
    public async Task HandleAsync_DecrementsAvailabilityAndRecordsInbox()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using (var seed = NewContext(dbName))
        {
            seed.Concerts.Add(NewConcert(totalTickets: 10));
            await seed.SaveChangesAsync();
        }
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new TicketPurchasedHandler(context).HandleAsync(NewEvent(), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.SingleAsync();
        Assert.Equal(9, concert.AvailableTickets);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPurchasedHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotDecrement()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(Base);
        await using (var seed = NewContext(dbName))
        {
            seed.Concerts.Add(NewConcert(totalTickets: 10));
            seed.AddInboxMessage(envelope, nameof(TicketPurchasedHandler));
            await seed.SaveChangesAsync();
        }

        // Act
        await using (var context = NewContext(dbName))
            await new TicketPurchasedHandler(context).HandleAsync(NewEvent(), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.SingleAsync();
        Assert.Equal(10, concert.AvailableTickets);
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_PersistsNothing()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new TicketPurchasedHandler(context).HandleAsync(NewEvent(concertId: 999), envelope);

        // Assert — the early return skips the save, so the inbox row is not consumed and a redelivery can retry
        await using var probe = NewContext(dbName);
        Assert.False(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPurchasedHandler)));
    }
}
