using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Payments;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Kernel.DependencyInjection;
using Concertable.Messaging.Contracts;
using Concertable.Messaging.Domain;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class TicketPaymentOutcomeTests(ApiFixture fixture, ITestOutputHelper output)
    : EventHandlerIntegrationTest(fixture, output)
{
    private readonly IScoped<TicketDbContext> scoped =
        fixture.Services.GetRequiredService<IScoped<TicketDbContext>>();

    [Fact]
    public async Task PaymentSucceeded_MultipleTickets_PersistsPeriodOnEveryTicket()
    {
        var concert = Fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = Fixture.SeedState.Customer2;
        var reference = CreateReference(buyer.Id, concert.Id, 2);
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(TestTime.Now);
        var @event = new PaymentSucceededEvent(reference, new Dictionary<string, string>());

        await DispatchAsync(@event, envelope);

        var tickets = await scoped.RunAsync(context =>
            context.Tickets.Where(ticket => ticket.UserId == buyer.Id).ToListAsync());
        tickets.Count.ShouldBe(2);
        tickets.ShouldAllBe(ticket => ticket.Period == concert.Period);
    }

    [Fact]
    public async Task PaymentSucceeded_OwnedReference_StagesReceiptForStoredCustomerEmail()
    {
        var concert = Fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = Fixture.SeedState.Customer1;
        var reference = CreateReference(buyer.Id, concert.Id, 2);
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(TestTime.Now);
        var @event = new PaymentSucceededEvent(reference, new Dictionary<string, string>());

        await DispatchAsync(@event, envelope);

        var emailCommand = MessageTypeAttribute.Resolve(typeof(SendTicketEmailCommand));
        var staged = await scoped.RunAsync(context =>
            context.Set<OutboxMessageEntity>()
                .Where(message => message.MessageType == emailCommand)
                .ToListAsync());
        var row = staged.ShouldHaveSingleItem();
        row.Payload.ShouldContain(buyer.Email);
    }

    [Fact]
    public async Task PaymentSucceeded_DuplicateEnvelope_CompletesPurchaseOnce()
    {
        var concert = Fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = Fixture.SeedState.Customer2;
        var reference = CreateReference(buyer.Id, concert.Id, 2);
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(TestTime.Now);
        var @event = new PaymentSucceededEvent(reference, new Dictionary<string, string>());

        await DispatchAsync(@event, envelope);
        await DispatchAsync(@event, envelope);

        var tickets = await scoped.RunAsync(context =>
            context.Tickets.Where(ticket => ticket.UserId == buyer.Id).ToListAsync());
        tickets.Count.ShouldBe(2);
        Fixture.NotificationClient.TicketPurchased.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task PaymentSucceeded_ForeignReference_SkipsWithoutRecordingInboxMessage()
    {
        var buyer = Fixture.SeedState.Customer2;
        var reference = new PaymentOperationReference("application-fee", "malformed");
        var envelope = MessageEnvelope.Create<PaymentSucceededEvent>(TestTime.Now);
        var @event = new PaymentSucceededEvent(reference, new Dictionary<string, string>());

        await DispatchAsync(@event, envelope);

        var state = await scoped.RunAsync(async context => new
        {
            TicketCount = await context.Tickets.CountAsync(ticket => ticket.UserId == buyer.Id),
            InboxRecorded = await context.Set<InboxMessageEntity>()
                .AnyAsync(message => message.MessageId == envelope.MessageId)
        });
        state.TicketCount.ShouldBe(0);
        state.InboxRecorded.ShouldBeFalse();
    }

    [Fact]
    public async Task PaymentFailed_DuplicateEnvelope_NotifiesOnceWithWholeReference()
    {
        var concert = Fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = Fixture.SeedState.Customer2;
        var reference = CreateReference(buyer.Id, concert.Id, 1);
        var envelope = MessageEnvelope.Create<PaymentFailedEvent>(TestTime.Now);
        var @event = new PaymentFailedEvent(reference, "declined", "Payment declined", new Dictionary<string, string>());

        await DispatchAsync(@event, envelope);
        await DispatchAsync(@event, envelope);

        var notification = Fixture.NotificationClient.Other.ShouldHaveSingleItem();
        notification.UserId.ShouldBe(buyer.Id.ToString());
        notification.EventName.ShouldBe("TicketPurchaseFailed");
        var payload = notification.Payload.ShouldBeOfType<TicketPaymentFailure>();
        payload.Reference.ShouldBe(reference);
        payload.FailureCode.ShouldBe("declined");
        payload.FailureMessage.ShouldBe("Payment declined");
    }

    private static PaymentOperationReference CreateReference(Guid buyerId, int concertId, int quantity) =>
        TicketPaymentOperationReferences.Create(
            TicketPaymentOperationType.Purchase,
            buyerId,
            concertId,
            quantity);
}
