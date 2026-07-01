using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.UnitTests.Handlers;

public sealed class VenueReadModelProjectionHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.NewGuid();

    private static ConcertDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<ConcertDbContext>().UseInMemoryDatabase(dbName).Options,
            new ConcertConfigurationProvider());

    private static VenueChangedEvent NewEvent(
        int venueId = 7,
        string name = "Venue",
        string town = "Guildford",
        double latitude = 51.5,
        double longitude = -0.1) =>
        new(venueId, UserId, name, "About", "avatar.png", "banner.png", "Surrey", town, latitude, longitude, "venue@test.com");

    [Fact]
    public async Task HandleAsync_WhenVenueUnknown_CreatesProjection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<VenueChangedEvent>(Base);
        var e = NewEvent();

        // Act
        await using (var context = NewContext(dbName))
            await new VenueReadModelProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var venue = await probe.VenueReadModels.SingleAsync();
        Assert.Equal(e.VenueId, venue.Id);
        Assert.Equal(e.Name, venue.Name);
        Assert.Equal(e.County, venue.Address.County);
        Assert.Equal(e.Town, venue.Address.Town);
        Assert.Equal(e.Latitude, venue.Latitude);
        Assert.Equal(e.Longitude, venue.Longitude);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(VenueReadModelProjectionHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenVenueExists_Updates()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using (var seed = NewContext(dbName))
            await new VenueReadModelProjectionHandler(seed).HandleAsync(
                NewEvent(name: "Old", town: "Oldtown"),
                MessageEnvelope.Create<VenueChangedEvent>(Base));
        var envelope = MessageEnvelope.Create<VenueChangedEvent>(Base);
        var e = NewEvent(name: "New", town: "Newtown", latitude: 52.2);

        // Act
        await using (var context = NewContext(dbName))
            await new VenueReadModelProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var venue = await probe.VenueReadModels.SingleAsync();
        Assert.Equal("New", venue.Name);
        Assert.Equal("Newtown", venue.Address.Town);
        Assert.Equal(52.2, venue.Latitude);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotApplyChanges()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<VenueChangedEvent>(Base);
        await using (var seed = NewContext(dbName))
        {
            await new VenueReadModelProjectionHandler(seed).HandleAsync(
                NewEvent(name: "Original"),
                MessageEnvelope.Create<VenueChangedEvent>(Base));
            seed.AddInboxMessage(envelope, nameof(VenueReadModelProjectionHandler));
            await seed.SaveChangesAsync();
        }

        // Act
        await using (var context = NewContext(dbName))
            await new VenueReadModelProjectionHandler(context).HandleAsync(NewEvent(name: "Renamed"), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var venue = await probe.VenueReadModels.SingleAsync();
        Assert.Equal("Original", venue.Name);
    }
}
