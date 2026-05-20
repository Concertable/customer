using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Events;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal class TicketPaymentProcessor : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly ITicketService ticketService;
    private readonly ITicketNotifier notifier;
    private readonly ILogger<TicketPaymentProcessor> logger;

    public TicketPaymentProcessor(ITicketService ticketService, ITicketNotifier notifier, ILogger<TicketPaymentProcessor> logger)
    {
        this.ticketService = ticketService;
        this.notifier = notifier;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (@event.Metadata.GetValueOrDefault("type") != TransactionTypes.Ticket)
            return;

        var meta = @event.Metadata;

        logger.LogInformation("[TicketPaymentProcessor] fromUserId={FromUserId}", meta["fromUserId"]);

        var result = await ticketService.CompleteAsync(new()
        {
            EntityId = int.Parse(meta["concertId"]),
            FromUserId = Guid.Parse(meta["fromUserId"]),
            FromEmail = meta.GetValueOrDefault("fromUserEmail", string.Empty),
            Quantity = meta.TryGetValue("quantity", out var q) ? int.Parse(q) : null
        });

        if (result.IsFailed)
            throw new BadRequestException(result.Errors);

        await notifier.TicketPurchasedAsync(meta["fromUserId"], result.Value);
    }
}
