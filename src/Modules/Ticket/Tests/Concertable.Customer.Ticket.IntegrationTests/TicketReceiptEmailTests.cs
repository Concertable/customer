using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Concertable.Messaging.Domain;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class TicketReceiptEmailTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public TicketReceiptEmailTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    #region CompleteFromPaymentSucceeded

    [Fact]
    public async Task PaymentSucceeded_StagesTicketReceiptEmailOnTheOutbox()
    {
        // Arrange
        const string buyerEmail = "receipt-test@concertable.test";
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = fixture.SeedState.Customer1;

        var @event = new PaymentSucceededEvent("pi_receipt_test", new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["fromUserId"] = buyer.Id.ToString(),
            ["fromUserEmail"] = buyerEmail,
            ["quantity"] = "2",
        });
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(DateTimeOffset.UtcNow);

        // Act
        using (var scope = fixture.Services.CreateScope())
            foreach (var handler in scope.ServiceProvider.GetServices<IIntegrationEventHandler<PaymentSucceededEvent>>())
                await handler.HandleAsync(@event, envelope);

        // Assert — the receipt send committed with the tickets as a SendTicketEmailCommand outbox row
        var emailCommand = MessageTypeAttribute.Resolve(typeof(SendTicketEmailCommand));

        using var probe = fixture.Services.CreateScope();
        var context = probe.ServiceProvider.GetRequiredService<TicketDbContext>();
        var staged = await context.Set<OutboxMessageEntity>()
            .Where(m => m.MessageType == emailCommand)
            .ToListAsync();

        var row = Assert.Single(staged);
        Assert.Contains(buyerEmail, row.Payload);
    }

    #endregion
}
