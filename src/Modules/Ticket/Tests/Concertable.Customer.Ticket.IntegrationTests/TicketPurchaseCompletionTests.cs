using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class TicketPurchaseCompletionTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public TicketPurchaseCompletionTests(ApiFixture fixture, ITestOutputHelper output)
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

    [Fact]
    public async Task PaymentSucceeded_MultipleTickets_PersistsPeriodOnEveryTicket()
    {
        // Arrange — a paid 2-ticket purchase arriving on the payment-succeeded webhook
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = fixture.SeedState.Customer2;
        var occurredAt = fixture.Services.GetRequiredService<TimeProvider>().GetUtcNow();
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(occurredAt);
        var @event = new PaymentSucceededEvent("pi_test_multi", new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["fromUserId"] = buyer.Id.ToString(),
            ["fromUserEmail"] = buyer.Email,
            ["quantity"] = "2"
        });

        // Act — through the registered handler (TicketPaymentProcessor), the real production entry point
        using (var scope = fixture.Services.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<PaymentSucceededEvent>>();
            await handler.HandleAsync(@event, envelope);
        }

        // Assert — read back in a fresh scope: both tickets persist, each with the concert's Period
        // (under the old OwnsOne mapping the shared Period instance left the 2nd ticket's Period NULL)
        using var probe = fixture.Services.CreateScope();
        var tickets = await probe.ServiceProvider.GetRequiredService<TicketDbContext>()
            .Tickets.Where(t => t.UserId == buyer.Id).ToListAsync();
        Assert.Equal(2, tickets.Count);
        Assert.All(tickets, t => Assert.Equal(concert.Period, t.Period));
    }
}
