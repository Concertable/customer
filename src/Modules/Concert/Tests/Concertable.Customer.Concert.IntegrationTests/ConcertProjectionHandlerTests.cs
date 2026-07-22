using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Contracts.Enums;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public sealed class ConcertProjectionHandlerTests : IAsyncLifetime
{
    private const int ConcertId = 90001;
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PayeeUserId = Guid.NewGuid();
    private static readonly Guid PayeeOwnerId = Guid.NewGuid();

    private readonly ApiFixture fixture;

    public ConcertProjectionHandlerTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();

    public Task DisposeAsync()
    {
        fixture.DetachOutput();
        return Task.CompletedTask;
    }

    private static ConcertChangedEvent NewEvent(
        string name = "Concert",
        int totalTickets = 10,
        IReadOnlyCollection<Genre>? genres = null) =>
        new(
            ConcertId,
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

    private async Task HandleAsync(ConcertChangedEvent e, MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        await new ConcertProjectionHandler(context).HandleAsync(e, envelope);
    }

    private async Task<ConcertEntity> GetConcertAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        return await context.Concerts.Include(c => c.Genres).SingleAsync(c => c.Id == ConcertId);
    }

    private async Task<bool> IsProcessedAsync(MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        return await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertProjectionHandler));
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_CreatesProjection()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(Base);
        var e = NewEvent(totalTickets: 10, genres: [Genre.Rock, Genre.Pop]);

        // Act
        await HandleAsync(e, envelope);

        // Assert
        var concert = await GetConcertAsync();
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
        Assert.True(await IsProcessedAsync(envelope));
    }

    [Fact]
    public async Task HandleAsync_WhenConcertExists_UpdatesPreservingSoldAndSyncsGenres()
    {
        // Arrange — 3 of 10 already sold; genres start as Rock+Pop
        await HandleAsync(
            NewEvent(totalTickets: 10, genres: [Genre.Rock, Genre.Pop]),
            MessageEnvelope.Create<ConcertChangedEvent>(Base));

        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
            var seeded = await context.Concerts.SingleAsync(c => c.Id == ConcertId);
            seeded.DecrementAvailability(3);
            await context.SaveChangesAsync();
        }

        // Act
        await HandleAsync(
            NewEvent(name: "Renamed", totalTickets: 20, genres: [Genre.Pop, Genre.Jazz]),
            MessageEnvelope.Create<ConcertChangedEvent>(Base));

        // Assert
        var concert = await GetConcertAsync();
        Assert.Equal("Renamed", concert.Name);
        Assert.Equal(20, concert.TotalTickets);
        Assert.Equal(17, concert.AvailableTickets);
        Assert.Equal([Genre.Pop, Genre.Jazz], concert.Genres.Select(g => g.Genre).Order());
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotApplyChanges()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<ConcertChangedEvent>(Base);
        await HandleAsync(NewEvent(name: "Original"), MessageEnvelope.Create<ConcertChangedEvent>(Base));

        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
            context.AddInboxMessage(envelope, nameof(ConcertProjectionHandler));
            await context.SaveChangesAsync();
        }

        // Act
        await HandleAsync(NewEvent(name: "Renamed"), envelope);

        // Assert
        var concert = await GetConcertAsync();
        Assert.Equal("Original", concert.Name);
    }
}
