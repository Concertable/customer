using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class TicketPurchaseCompletionTests(ApiFixture fixture, ITestOutputHelper output)
    : EventHandlerIntegrationTest(fixture, output)
{
    private readonly IScoped<TicketDbContext> scoped = fixture.Services.GetRequiredService<IScoped<TicketDbContext>>();

    [Fact]
    public async Task PaymentSucceeded_MultipleTickets_PersistsPeriodOnEveryTicket()
    {
        // a paid 2-ticket purchase arriving on the payment-succeeded webhook
        var concert = Fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = Fixture.SeedState.Customer2;
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(TestTime.Now);
        var @event = new PaymentSucceededEvent("pi_test_multi", new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["fromUserId"] = buyer.Id.ToString(),
            ["fromUserEmail"] = buyer.Email,
            ["quantity"] = "2"
        });

        await DispatchAsync(@event, envelope);

        // both tickets persist, each with the concert's Period
        // (under the old OwnsOne mapping the shared Period instance left the 2nd ticket's Period NULL)
        var tickets = await scoped.RunAsync(ctx => ctx.Tickets.Where(t => t.UserId == buyer.Id).ToListAsync());
        Assert.Equal(2, tickets.Count);
        Assert.All(tickets, t => Assert.Equal(concert.Period, t.Period));
    }
}
