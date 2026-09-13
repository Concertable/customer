using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.UnitTests.Handlers;

public sealed class ArtistRatingReadModelProjectionHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ConcertDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<ConcertDbContext>().UseInMemoryDatabase(dbName).Options,
            new ConcertConfigurationProvider());

    private static async Task SeedArtistAsync(string dbName, int artistId)
    {
        await using var context = NewContext(dbName);
        context.ArtistReadModels.Add(new ArtistReadModel
        {
            Id = artistId,
            Name = "Artist",
            Avatar = "avatar.png",
            Address = new Address("Surrey", "Guildford")
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenArtistExists_UpdatesRating()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await SeedArtistAsync(dbName, 5);
        var envelope = MessageEnvelope.Create<ArtistRatingUpdatedEvent>(Base);
        var e = new ArtistRatingUpdatedEvent(5, 4.25, 8);

        // Act
        await using (var context = NewContext(dbName))
            await new ArtistRatingReadModelProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var artist = await probe.ArtistReadModels.SingleAsync();
        Assert.Equal(4.25, artist.AverageRating);
        Assert.Equal(8, artist.ReviewCount);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ArtistRatingReadModelProjectionHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenArtistUnknown_NoOp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ArtistRatingUpdatedEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new ArtistRatingReadModelProjectionHandler(context).HandleAsync(new ArtistRatingUpdatedEvent(99, 5, 3), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        Assert.False(await probe.ArtistReadModels.AnyAsync());
    }
}
