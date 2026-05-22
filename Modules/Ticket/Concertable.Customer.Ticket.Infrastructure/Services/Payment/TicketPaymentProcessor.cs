using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Concertable.Messaging.Domain;
using Concertable.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal class TicketPaymentProcessor : IIntegrationEventHandler<PaymentSucceededEvent>
{
    private readonly ITicketService ticketService;
    private readonly ITicketNotifier notifier;
    private readonly TicketDbContext context;
    private readonly ILogger<TicketPaymentProcessor> logger;

    public TicketPaymentProcessor(
        ITicketService ticketService,
        ITicketNotifier notifier,
        TicketDbContext context,
        ILogger<TicketPaymentProcessor> logger)
    {
        this.ticketService = ticketService;
        this.notifier = notifier;
        this.context = context;
        this.logger = logger;
    }

    public async Task HandleAsync(PaymentSucceededEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (@event.Metadata.GetValueOrDefault("type") != TransactionTypes.Ticket)
            return;

        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(TicketPaymentProcessor), ct))
            return;

        var meta = @event.Metadata;

        logger.LogInformation("[TicketPaymentProcessor] fromUserId={FromUserId}", meta["fromUserId"]);

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(TicketPaymentProcessor), envelope.MessageType, DateTimeOffset.UtcNow));

        try
        {
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
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            logger.LogDebug("Duplicate inbox message {MessageId}; skipping", envelope.MessageId);
        }
    }
}
