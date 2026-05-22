using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal class TicketPaymentFailedProcessor : IIntegrationEventHandler<PaymentFailedEvent>
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
        if (@event.Metadata.GetValueOrDefault("type") != TransactionTypes.Ticket)
            return;

        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(TicketPaymentFailedProcessor), ct))
            return;

        var fromUserId = @event.Metadata["fromUserId"];
        logger.LogWarning(
            "Ticket payment failed for user {UserId}: [{FailureCode}] {FailureMessage}",
            fromUserId, @event.FailureCode, @event.FailureMessage);

        await notifier.TicketPurchaseFailedAsync(fromUserId, new { @event.FailureCode, @event.FailureMessage });

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(TicketPaymentFailedProcessor), envelope.MessageType, DateTimeOffset.UtcNow));
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            logger.LogDebug("Duplicate inbox message {MessageId}; skipping", envelope.MessageId);
        }
    }
}
