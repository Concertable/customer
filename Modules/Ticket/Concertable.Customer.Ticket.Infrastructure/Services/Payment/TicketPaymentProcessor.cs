using Concertable.Concert.Application.Interfaces;
using Concertable.Payment.Contracts.Events;
using Concertable.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

// TEMPORARY Phase 1: implements Concert-internal IPaymentSucceededProcessor via cross-IVT. Replaced in Phase 2
// Step 8 by an IConsumer<PaymentSucceededEvent> (MassTransit) — Payment publishes, Customer.Ticket subscribes
// directly. The keyed-dispatch pattern + this interface both disappear.
internal class TicketPaymentProcessor : IPaymentSucceededProcessor
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

    public async Task HandleAsync(PaymentSucceededEvent @event, CancellationToken ct)
    {
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
