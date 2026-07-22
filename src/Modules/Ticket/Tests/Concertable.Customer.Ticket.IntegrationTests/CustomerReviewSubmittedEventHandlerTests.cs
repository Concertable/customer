using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Services.Events;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class CustomerReviewSubmittedEventHandlerTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private readonly ApiFixture fixture;

    public CustomerReviewSubmittedEventHandlerTests(ApiFixture fixture, ITestOutputHelper output)
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

    private static TicketEntity NewTicket(Guid ticketId) =>
        TicketEntity.Create(
            ticketId, Guid.NewGuid(), 1, [1, 2, 3], Base.UtcDateTime,
            "Concert", 25m,
            new DateRange(Base.UtcDateTime.AddDays(-7), Base.UtcDateTime.AddDays(-6)),
            5, "Artist", 7, "Venue");

    private static CustomerReviewSubmittedEvent NewEvent(Guid ticketId) =>
        new(ticketId, 5, 7, 1, 4, "customer@test.com", "Great show");

    private async Task SeedTicketAsync(Guid ticketId, MessageEnvelope? processed = null)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        context.Tickets.Add(NewTicket(ticketId));
        if (processed is not null)
            context.AddInboxMessage(processed, nameof(CustomerReviewSubmittedEventHandler));
        await context.SaveChangesAsync();
    }

    private async Task HandleAsync(CustomerReviewSubmittedEvent e, MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var handler = ActivatorUtilities.CreateInstance<CustomerReviewSubmittedEventHandler>(scope.ServiceProvider);
        await handler.HandleAsync(e, envelope);
    }

    private async Task<TicketEntity> GetTicketAsync(Guid ticketId)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        return await context.Tickets.SingleAsync(t => t.Id == ticketId);
    }

    private async Task<bool> IsProcessedAsync(MessageEnvelope envelope)
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        return await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(CustomerReviewSubmittedEventHandler));
    }

    [Fact]
    public async Task HandleAsync_MarksTicketReviewedAndRecordsInbox()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(Base);
        await SeedTicketAsync(ticketId);

        // Act
        await HandleAsync(NewEvent(ticketId), envelope);

        // Assert
        var stored = await GetTicketAsync(ticketId);
        Assert.True(stored.HasReview);
        Assert.True(await IsProcessedAsync(envelope));
    }

    [Fact]
    public async Task HandleAsync_WhenTicketMissing_StillRecordsInbox()
    {
        // Arrange
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(Base);

        // Act
        await HandleAsync(NewEvent(Guid.NewGuid()), envelope);

        // Assert — the miss is logged and the message is consumed, not retried
        Assert.True(await IsProcessedAsync(envelope));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotMarkReviewed()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(Base);
        await SeedTicketAsync(ticketId, processed: envelope);

        // Act
        await HandleAsync(NewEvent(ticketId), envelope);

        // Assert
        var stored = await GetTicketAsync(ticketId);
        Assert.False(stored.HasReview);
    }
}
