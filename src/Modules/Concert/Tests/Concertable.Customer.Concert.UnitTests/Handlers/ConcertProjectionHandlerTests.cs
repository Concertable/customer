using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Contracts;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.UnitTests.Handlers;

public sealed class ConcertProjectionHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PayeeUserId = Guid.NewGuid();
    private static readonly Guid PayeeOwnerId = Guid.NewGuid();

    private static ConcertDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<ConcertDbContext>().UseInMemoryDatabase(dbName).Options,
            new ConcertConfigurationProvider());

    private static ConcertChangedEvent NewEvent(
        int concertId = 1,
        string name = "Concert",
        int totalTickets = 10,
        IReadOnlyCollection<Genre>? genres = null) =>
        new(
            concertId,
            name,
            "About",
            "avatar.png",
            "banner.png",
            totalTickets,
            totalTickets,
            25m,
            new DateRange(Base.UtcDateTime.AddDays(30), Base.UtcDateTime.AddDays(31)),
            Base.UtcDateTime,
            5,
            "Artist",
            7,
            "Venue",
            51.5,
            -0.1,
            genres ?? [Genre.Rock],
            PayeeUserId,
            PayeeOwnerId);

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_CreatesProjection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(Base);
        var e = NewEvent(totalTickets: 10, genres: [Genre.Rock, Genre.Pop]);

        // Act
        await using (var context = NewContext(dbName))
            await new ConcertProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.Include(c => c.Genres).SingleAsync();
        Assert.Equal(e.ConcertId, concert.Id);
        Assert.Equal(e.Name, concert.Name);
        Assert.Equal(e.About, concert.About);
        Assert.Equal(e.BannerUrl, concert.BannerUrl);
        Assert.Equal(e.Avatar, concert.Avatar);
        Assert.Equal(10, concert.TotalTickets);
        Assert.Equal(10, concert.AvailableTickets);
        Assert.Equal(e.Price, concert.Price);
        Assert.Equal(e.Period, concert.Period);
        Assert.Equal(e.ArtistId, concert.ArtistId);
        Assert.Equal(e.ArtistName, concert.ArtistName);
        Assert.Equal(e.VenueId, concert.VenueId);
        Assert.Equal(e.VenueName, concert.VenueName);
        Assert.Equal(e.PayeeUserId, concert.PayeeUserId);
        Assert.Equal(e.PayeeOwnerId, concert.PayeeOwnerId);
        Assert.Equal([Genre.Rock, Genre.Pop], concert.Genres.Select(g => g.Genre).Order());
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertProjectionHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenConcertExists_UpdatesPreservingSoldAndSyncsGenres()
    {
        // Arrange — 3 of 10 already sold; genres start as Rock+Pop
        var dbName = Guid.NewGuid().ToString();
        await using (var seed = NewContext(dbName))
        {
            await new ConcertProjectionHandler(seed).HandleAsync(
                NewEvent(totalTickets: 10, genres: [Genre.Rock, Genre.Pop]),
                MessageEnvelope.Create<ConcertChangedEvent>(Base));
            var seeded = await seed.Concerts.SingleAsync();
            seeded.DecrementAvailability(3);
            await seed.SaveChangesAsync();
        }
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(Base);
        var e = NewEvent(name: "Renamed", totalTickets: 20, genres: [Genre.Pop, Genre.Jazz]);

        // Act
        await using (var context = NewContext(dbName))
            await new ConcertProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.Include(c => c.Genres).SingleAsync();
        Assert.Equal("Renamed", concert.Name);
        Assert.Equal(20, concert.TotalTickets);
        Assert.Equal(17, concert.AvailableTickets);
        Assert.Equal([Genre.Pop, Genre.Jazz], concert.Genres.Select(g => g.Genre).Order());
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotApplyChanges()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(Base);
        await using (var seed = NewContext(dbName))
        {
            await new ConcertProjectionHandler(seed).HandleAsync(
                NewEvent(name: "Original"),
                MessageEnvelope.Create<ConcertChangedEvent>(Base));
            seed.AddInboxMessage(envelope, nameof(ConcertProjectionHandler));
            await seed.SaveChangesAsync();
        }

        // Act
        await using (var context = NewContext(dbName))
            await new ConcertProjectionHandler(context).HandleAsync(NewEvent(name: "Renamed"), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var concert = await probe.Concerts.SingleAsync();
        Assert.Equal("Original", concert.Name);
    }
}
