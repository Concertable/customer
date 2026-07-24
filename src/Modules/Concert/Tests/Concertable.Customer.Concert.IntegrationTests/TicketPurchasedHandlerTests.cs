using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public sealed class TicketPurchasedHandlerTests(ApiFixture fixture, ITestOutputHelper output)
    : EventHandlerIntegrationTest(fixture, output)
{
    private const int ConcertId = 90003;
    private const int UnknownConcertId = 90998;
    private readonly IScoped<ConcertDbContext> scoped = fixture.Services.GetRequiredService<IScoped<ConcertDbContext>>();

    private static TicketPurchasedEvent NewEvent(int concertId = ConcertId) =>
        new(Guid.NewGuid(), Guid.NewGuid(), concertId, 25m, TestTime.Now.UtcDateTime);

    private Task SeedConcertAsync() =>
        DispatchAsync(ConcertChangedEvents.Create(ConcertId), MessageEnvelope.Create<ConcertChangedEvent>(TestTime.Now));

    [Fact]
    public async Task HandleAsync_DecrementsAvailabilityAndRecordsInbox()
    {
        await SeedConcertAsync();
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(TestTime.Now);

        await DispatchAsync(NewEvent(), envelope);

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.SingleAsync(c => c.Id == ConcertId));
        Assert.Equal(9, concert.AvailableTickets);
        Assert.True(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPurchasedHandler))));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotDecrement()
    {
        await SeedConcertAsync();
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(TestTime.Now);
        await scoped.RunAsync(async ctx =>
        {
            ctx.AddInboxMessage(envelope, nameof(TicketPurchasedHandler));
            await ctx.SaveChangesAsync();
        });

        await DispatchAsync(NewEvent(), envelope);

        var concert = await scoped.RunAsync(ctx => ctx.Concerts.SingleAsync(c => c.Id == ConcertId));
        Assert.Equal(10, concert.AvailableTickets);
    }

    [Fact]
    public async Task HandleAsync_WhenConcertUnknown_PersistsNothing()
    {
        var envelope = MessageEnvelope.Create<TicketPurchasedEvent>(TestTime.Now);

        await DispatchAsync(NewEvent(concertId: UnknownConcertId), envelope);

        // the early return skips the save, so the inbox row is not consumed and a redelivery can retry
        Assert.False(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPurchasedHandler))));
    }
}
