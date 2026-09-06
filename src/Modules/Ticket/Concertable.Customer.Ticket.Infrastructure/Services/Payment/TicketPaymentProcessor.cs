using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Payments;
using Concertable.Customer.Ticket.Infrastructure;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal sealed class TicketPaymentProcessor : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly ITicketService ticketService;
    private readonly ITicketNotifier notifier;
    private readonly IUserModule userModule;
    private readonly TicketDbContext context;
    private readonly ILogger<TicketPaymentProcessor> logger;

    public TicketPaymentProcessor(
        ITicketService ticketService,
        ITicketNotifier notifier,
        IUserModule userModule,
        TicketDbContext context,
        ILogger<TicketPaymentProcessor> logger)
    {
        this.ticketService = ticketService;
        this.notifier = notifier;
        this.userModule = userModule;
        this.context = context;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (!@event.Reference.TryGetPurchase(out var purchase))
            return;

        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPaymentProcessor), ct))
            return;

        var userId = purchase.BuyerId.ToString();
        logger.TicketPaymentProcessing(userId);

        context.AddInboxMessage(envelope, nameof(TicketPaymentProcessor));

        try
        {
            var customers = await userModule.GetByIdsAsync([purchase.BuyerId]);
            var customer = customers.SingleOrDefault();
            var payment = await ticketService.CompleteAsync(new()
            {
                Reference = @event.Reference,
                EntityId = purchase.ConcertId,
                FromUserId = purchase.BuyerId,
                FromEmail = customer?.Email,
                Quantity = purchase.Quantity
            });

            await notifier.TicketPurchasedAsync(userId, payment);
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            logger.DuplicateInboxMessage(envelope.MessageId);
        }
    }
}
