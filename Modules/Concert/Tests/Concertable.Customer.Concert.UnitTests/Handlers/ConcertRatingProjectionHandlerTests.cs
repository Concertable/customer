using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.UnitTests.Handlers;

public sealed class ConcertRatingProjectionHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ConcertDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<ConcertDbContext>().UseInMemoryDatabase(dbName).Options,
            new ConcertConfigurationProvider());

    private static ConcertEntity NewConcert(int concertId = 1) =>
        ConcertEntity.Create(
            concertId, "Concert", "About", "banner.png", "avatar.png",
            10, 25m,
            new DateRange(Base.UtcDateTime.AddDays(30), Base.UtcDateTime.AddDays(31)),
            Base.UtcDateTime,
            5, "Artist", 7, "Venue", Guid.NewGuid());

    [Fact]
    public async Task HandleAsync_UpdatesRatingAndRecordsInbox()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using (var seed = NewContext(dbName))
        {
            seed.Concerts.Add(NewConcert());
            await seed.SaveChangesAsync();
        }
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new ConcertRatingProjectionHandler(context).HandleAsync(
                new ConcertRatingUpdatedEvent(1, 4.5, 12), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.SingleAsync();
        Assert.Equal(4.5, concert.AverageRating);
        Assert.Equal(12, concert.ReviewCount);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertRatingProjectionHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotUpdate()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(Base);
        await using (var seed = NewContext(dbName))
        {
            seed.Concerts.Add(NewConcert());
            seed.AddInboxMessage(envelope, nameof(ConcertRatingProjectionHandler));
            await seed.SaveChangesAsync();
        }

        // Act
        await using (var context = NewContext(dbName))
            await new ConcertRatingProjectionHandler(context).HandleAsync(
                new ConcertRatingUpdatedEvent(1, 4.5, 12), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.SingleAsync();
        Assert.Equal(0, concert.AverageRating);
        Assert.Equal(0, concert.ReviewCount);
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_PersistsNothing()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new ConcertRatingProjectionHandler(context).HandleAsync(
                new ConcertRatingUpdatedEvent(999, 4.5, 12), envelope);

        // Assert — the early return skips the save, so the inbox row is not consumed and a redelivery can retry
        await using var probe = NewContext(dbName);
        Assert.False(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertRatingProjectionHandler)));
    }
}
