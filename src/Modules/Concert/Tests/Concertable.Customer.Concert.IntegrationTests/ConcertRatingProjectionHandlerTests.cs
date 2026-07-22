using Concertable.B2B.Concert.Contracts.Events;
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
public sealed class ConcertRatingProjectionHandlerTests : IAsyncLifetime
{
    private const int ConcertId = 90002;
    private const int UnknownConcertId = 90999;
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly ApiFixture fixture;

    public ConcertRatingProjectionHandlerTests(ApiFixture fixture, ITestOutputHelper output)
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

    private static ConcertEntity NewConcert() =>
        ConcertEntity.Create(
            ConcertId, "Concert", "About", "banner.png", "avatar.png",
            10, 25m,
            new DateRange(Base.UtcDateTime.AddDays(30), Base.UtcDateTime.AddDays(31)),
            Base.UtcDateTime,
            5, "Artist", 7, "Venue", Guid.NewGuid(), Guid.NewGuid());

    private async Task SeedConcertAsync(MessageEnvelope? processed = null)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        context.Concerts.Add(NewConcert());
        if (processed is not null)
            context.AddInboxMessage(processed, nameof(ConcertRatingProjectionHandler));
        await context.SaveChangesAsync();
    }

    private async Task HandleAsync(ConcertRatingUpdatedEvent e, MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        await new ConcertRatingProjectionHandler(context).HandleAsync(e, envelope);
    }

    private async Task<ConcertEntity> GetConcertAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        return await context.Concerts.SingleAsync(c => c.Id == ConcertId);
    }

    private async Task<bool> IsProcessedAsync(MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        return await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertRatingProjectionHandler));
    }

    [Fact]
    public async Task HandleAsync_UpdatesRatingAndRecordsInbox()
    {
        // Arrange
        await SeedConcertAsync();
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(Base);

        // Act
        await HandleAsync(new ConcertRatingUpdatedEvent(ConcertId, 4.5, 12), envelope);

        // Assert
        var concert = await GetConcertAsync();
        Assert.Equal(4.5, concert.AverageRating);
        Assert.Equal(12, concert.ReviewCount);
        Assert.True(await IsProcessedAsync(envelope));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotUpdate()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(Base);
        await SeedConcertAsync(processed: envelope);

        // Act
        await HandleAsync(new ConcertRatingUpdatedEvent(ConcertId, 4.5, 12), envelope);

        // Assert
        var concert = await GetConcertAsync();
        Assert.Equal(0, concert.AverageRating);
        Assert.Equal(0, concert.ReviewCount);
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_PersistsNothing()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<ConcertRatingUpdatedEvent>(Base);

        // Act
        await HandleAsync(new ConcertRatingUpdatedEvent(UnknownConcertId, 4.5, 12), envelope);

        // Assert — the early return skips the save, so the inbox row is not consumed and a redelivery can retry
        Assert.False(await IsProcessedAsync(envelope));
    }
}
