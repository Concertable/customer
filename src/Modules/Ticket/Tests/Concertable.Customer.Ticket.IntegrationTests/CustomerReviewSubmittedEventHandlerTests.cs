using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Services.Events;
using Concertable.Kernel.DependencyInjection;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class CustomerReviewSubmittedEventHandlerTests(ApiFixture fixture, ITestOutputHelper output)
    : EventHandlerIntegrationTest(fixture, output)
{
    private readonly IScoped<TicketDbContext> scoped = fixture.Services.GetRequiredService<IScoped<TicketDbContext>>();

    private static TicketEntity NewTicket(Guid ticketId) =>
        TicketEntity.Create(
            ticketId, Guid.NewGuid(), 1, [1, 2, 3], TestTime.Now.UtcDateTime,
            "Concert", 25m,
            new DateRange(TestTime.Now.UtcDateTime.AddDays(-7), TestTime.Now.UtcDateTime.AddDays(-6)),
            5, "Artist", 7, "Venue");

    private static CustomerReviewSubmittedEvent NewEvent(Guid ticketId) =>
        new(ticketId, 5, 7, 1, 4, "customer@test.com", "Great show");

    private Task SeedTicketAsync(Guid ticketId, MessageEnvelope? processed = null) =>
        scoped.RunAsync(async ctx =>
        {
            ctx.Tickets.Add(NewTicket(ticketId));
            if (processed is not null)
                ctx.AddInboxMessage(processed, nameof(CustomerReviewSubmittedEventHandler));
            await ctx.SaveChangesAsync();
        });

    [Fact]
    public async Task HandleAsync_MarksTicketReviewedAndRecordsInbox()
    {
        var ticketId = Guid.NewGuid();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(TestTime.Now);
        await SeedTicketAsync(ticketId);

        await DispatchAsync(NewEvent(ticketId), envelope);

        var stored = await scoped.RunAsync(ctx => ctx.Tickets.SingleAsync(t => t.Id == ticketId));
        Assert.True(stored.HasReview);
        Assert.True(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(CustomerReviewSubmittedEventHandler))));
    }

    [Fact]
    public async Task HandleAsync_WhenTicketMissing_StillRecordsInbox()
    {
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(TestTime.Now);

        await DispatchAsync(NewEvent(Guid.NewGuid()), envelope);

        // the miss is logged and the message is consumed, not retried
        Assert.True(await scoped.RunAsync(ctx => ctx.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(CustomerReviewSubmittedEventHandler))));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotMarkReviewed()
    {
        var ticketId = Guid.NewGuid();
        var envelope = MessageEnvelope.Create<CustomerReviewSubmittedEvent>(TestTime.Now);
        await SeedTicketAsync(ticketId, processed: envelope);

        await DispatchAsync(NewEvent(ticketId), envelope);

        var stored = await scoped.RunAsync(ctx => ctx.Tickets.SingleAsync(t => t.Id == ticketId));
        Assert.False(stored.HasReview);
    }
}
