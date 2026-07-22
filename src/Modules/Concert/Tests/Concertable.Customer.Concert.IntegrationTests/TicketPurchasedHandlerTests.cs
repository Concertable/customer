using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public sealed class TicketPurchasedHandlerTests : IAsyncLifetime
{
    private const int ConcertId = 90003;
    private const int UnknownConcertId = 90998;
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PayeeUserId = Guid.NewGuid();
    private static readonly Guid PayeeOwnerId = Guid.NewGuid();

    private readonly ApiFixture fixture;

    public TicketPurchasedHandlerTests(ApiFixture fixture, ITestOutputHelper output)
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

    private static ConcertEntity NewConcert(int totalTickets = 10) =>
        ConcertEntity.Create(
            ConcertId, "Concert", "About", "banner.png", "avatar.png",
            totalTickets, 25m,
            new DateRange(Base.UtcDateTime.AddDays(30), Base.UtcDateTime.AddDays(31)),
            Base.UtcDateTime,
            5, "Artist", 7, "Venue", PayeeUserId, PayeeOwnerId);

    private static TicketPurchasedEvent NewEvent(int concertId = ConcertId) =>
        new(Guid.NewGuid(), Guid.NewGuid(), concertId, 25m, Base.UtcDateTime);

    private async Task SeedConcertAsync(MessageEnvelope? processed = null)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        context.Concerts.Add(NewConcert());
        if (processed is not null)
            context.AddInboxMessage(processed, nameof(TicketPurchasedHandler));
        await context.SaveChangesAsync();
    }

    private async Task HandleAsync(TicketPurchasedEvent e, MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConcertDbContext>();
        await new TicketPurchasedHandler(context).HandleAsync(e, envelope);
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
        return await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPurchasedHandler));
    }

    [Fact]
    public async Task HandleAsync_DecrementsAvailabilityAndRecordsInbox()
    {
        // Arrange
        await SeedConcertAsync();
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(Base);

        // Act
        await HandleAsync(NewEvent(), envelope);

        // Assert
        var concert = await GetConcertAsync();
        Assert.Equal(9, concert.AvailableTickets);
        Assert.True(await IsProcessedAsync(envelope));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotDecrement()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(Base);
        await SeedConcertAsync(processed: envelope);

        // Act
        await HandleAsync(NewEvent(), envelope);

        // Assert
        var concert = await GetConcertAsync();
        Assert.Equal(10, concert.AvailableTickets);
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_PersistsNothing()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(Base);

        // Act
        await HandleAsync(NewEvent(concertId: UnknownConcertId), envelope);

        // Assert — the early return skips the save, so the inbox row is not consumed and a redelivery can retry
        Assert.False(await IsProcessedAsync(envelope));
    }
}
