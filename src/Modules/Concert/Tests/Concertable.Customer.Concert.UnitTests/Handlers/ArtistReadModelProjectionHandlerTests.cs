using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Contracts.Enums;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.UnitTests.Handlers;

public sealed class ArtistReadModelProjectionHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TenantId = Guid.NewGuid();

    private static ConcertDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<ConcertDbContext>().UseInMemoryDatabase(dbName).Options,
            new ConcertConfigurationProvider());

    private static ArtistChangedEvent NewEvent(
        int artistId = 5,
        string name = "Artist",
        string avatar = "avatar.png",
        IReadOnlyCollection<Genre>? genres = null) =>
        new(artistId, UserId, name, "About", avatar, "banner.png", "Surrey", "Guildford", 51.5, -0.1, "artist@test.com",
            genres ?? [Genre.Rock], TenantId);

    [Fact]
    public async Task HandleAsync_WhenArtistUnknown_CreatesProjectionWithGenres()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ArtistChangedEvent>(Base);
        var e = NewEvent(genres: [Genre.Rock, Genre.Pop]);

        // Act
        await using (var context = NewContext(dbName))
            await new ArtistReadModelProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var artist = await probe.ArtistReadModels.Include(a => a.Genres).SingleAsync();
        Assert.Equal(e.ArtistId, artist.Id);
        Assert.Equal(e.Name, artist.Name);
        Assert.Equal(e.Avatar, artist.Avatar);
        Assert.Equal(e.County, artist.Address.County);
        Assert.Equal(e.Town, artist.Address.Town);
        Assert.Equal([Genre.Rock, Genre.Pop], artist.Genres.Select(g => g.Genre).Order());
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ArtistReadModelProjectionHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenArtistExists_SyncsGenresAndPreservesRating()
    {
        // Arrange — rating is owned by ArtistRatingUpdatedEvent, so ArtistChangedEvent must not clobber it
        var dbName = Guid.NewGuid().ToString();
        await using (var seed = NewContext(dbName))
        {
            await new ArtistReadModelProjectionHandler(seed).HandleAsync(
                NewEvent(genres: [Genre.Rock, Genre.Pop]),
                MessageEnvelope.Create<ArtistChangedEvent>(Base));
            var seeded = await seed.ArtistReadModels.SingleAsync();
            seeded.AverageRating = 4.5;
            seeded.ReviewCount = 12;
            await seed.SaveChangesAsync();
        }
        var envelope = MessageEnvelope.Create<ArtistChangedEvent>(Base);
        var e = NewEvent(name: "Renamed", genres: [Genre.Pop, Genre.Jazz]);

        // Act
        await using (var context = NewContext(dbName))
            await new ArtistReadModelProjectionHandler(context).HandleAsync(e, envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var artist = await probe.ArtistReadModels.Include(a => a.Genres).SingleAsync();
        Assert.Equal("Renamed", artist.Name);
        Assert.Equal([Genre.Pop, Genre.Jazz], artist.Genres.Select(g => g.Genre).Order());
        Assert.Equal(4.5, artist.AverageRating);
        Assert.Equal(12, artist.ReviewCount);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotApplyChanges()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<ArtistChangedEvent>(Base);
        await using (var seed = NewContext(dbName))
        {
            await new ArtistReadModelProjectionHandler(seed).HandleAsync(
                NewEvent(name: "Original"),
                MessageEnvelope.Create<ArtistChangedEvent>(Base));
            seed.AddInboxMessage(envelope, nameof(ArtistReadModelProjectionHandler));
            await seed.SaveChangesAsync();
        }

        // Act
        await using (var context = NewContext(dbName))
            await new ArtistReadModelProjectionHandler(context).HandleAsync(NewEvent(name: "Renamed"), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var artist = await probe.ArtistReadModels.SingleAsync();
        Assert.Equal("Original", artist.Name);
    }
}
