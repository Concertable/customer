using Concertable.Customer.Ticket.Infrastructure;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Concertable.Messaging.Contracts;
using Concertable.Kernel.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Payment;

internal sealed class TicketPaymentProcessor : IIntegrationEventHandler<PaymentSucceededEvent>
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
        if (@event.Metadata.GetValueOrDefault(PaymentMetadataKeys.Type) != TransactionTypes.Ticket)
            return;

        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPaymentProcessor), ct))
            return;

        var meta = @event.Metadata;

        logger.TicketPaymentProcessing(meta.GetValue(PaymentMetadataKeys.FromUserId));

        context.AddInboxMessage(envelope, nameof(TicketPaymentProcessor));

        try
        {
            var result = await ticketService.CompleteAsync(new()
            {
                EntityId = meta.GetValueAs<int>(PaymentMetadataKeys.ConcertId),
                FromUserId = meta.GetValueAs<Guid>(PaymentMetadataKeys.FromUserId),
                FromEmail = meta.GetValue(PaymentMetadataKeys.FromUserEmail),
                Quantity = meta.TryGetValue(PaymentMetadataKeys.Quantity, out var q) ? int.Parse(q) : null
            });

            if (result.IsFailed)
                throw new BadRequestException(result.Errors);

            await notifier.TicketPurchasedAsync(meta.GetValue(PaymentMetadataKeys.FromUserId), result.Value);
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKey())
        {
            logger.DuplicateInboxMessage(envelope.MessageId);
        }
    }
}
