using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal class TicketPaymentFailedProcessor : IIntegrationEventHandler<PaymentFailedEvent>
{
    private readonly ITicketNotifier notifier;
    private readonly ILogger<TicketPaymentFailedProcessor> logger;

    public TicketPaymentFailedProcessor(ITicketNotifier notifier, ILogger<TicketPaymentFailedProcessor> logger)
    {
        this.notifier = notifier;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentFailedEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (@event.Metadata.GetValueOrDefault("type") != TransactionTypes.Ticket)
            return;

        var fromUserId = @event.Metadata["fromUserId"];
        logger.LogWarning(
            "Ticket payment failed for user {UserId}: [{FailureCode}] {FailureMessage}",
            fromUserId, @event.FailureCode, @event.FailureMessage);
        await notifier.TicketPurchaseFailedAsync(fromUserId, new { @event.FailureCode, @event.FailureMessage });
    }
}
