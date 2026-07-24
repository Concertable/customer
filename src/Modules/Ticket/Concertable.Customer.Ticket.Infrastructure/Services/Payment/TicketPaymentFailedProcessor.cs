using Concertable.Customer.Ticket.Infrastructure;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal sealed class TicketPaymentFailedProcessor : IIntegrationEventHandler<PaymentFailedEvent>
{
    private readonly ITicketNotifier notifier;
    private readonly TicketDbContext context;
    private readonly ILogger<TicketPaymentFailedProcessor> logger;

    public TicketPaymentFailedProcessor(
        ITicketNotifier notifier,
        TicketDbContext context,
        ILogger<TicketPaymentFailedProcessor> logger)
    {
        this.notifier = notifier;
        this.context = context;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentFailedEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (@event.Metadata.GetValueOrDefault(PaymentMetadataKeys.Type) != TransactionTypes.Ticket)
            return;

        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPaymentFailedProcessor), ct))
            return;

        var fromUserId = @event.Metadata.GetValue(PaymentMetadataKeys.FromUserId);
        logger.TicketPaymentFailed(fromUserId, @event.FailureCode, @event.FailureMessage);

        await notifier.TicketPurchaseFailedAsync(fromUserId, new { @event.FailureCode, @event.FailureMessage });

        context.AddInboxMessage(envelope, nameof(TicketPaymentFailedProcessor));
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            logger.DuplicateInboxMessage(envelope.MessageId);
        }
    }
}
