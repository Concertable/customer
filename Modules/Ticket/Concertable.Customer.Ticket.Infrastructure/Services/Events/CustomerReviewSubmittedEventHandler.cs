using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Infrastructure;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.DataAccess.Infrastructure.Extensions;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services.Events;

internal sealed class CustomerReviewSubmittedEventHandler : IIntegrationEventHandler<CustomerReviewSubmittedEvent>
{
    private readonly ITicketRepository ticketRepository;
    private readonly TicketDbContext context;
    private readonly ILogger<CustomerReviewSubmittedEventHandler> logger;

    public CustomerReviewSubmittedEventHandler(
        ITicketRepository ticketRepository,
        TicketDbContext context,
        ILogger<CustomerReviewSubmittedEventHandler> logger)
    {
        this.ticketRepository = ticketRepository;
        this.context = context;
        this.logger = logger;
    }

    public async Task HandleAsync(CustomerReviewSubmittedEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(CustomerReviewSubmittedEventHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(CustomerReviewSubmittedEventHandler));

        var ticket = await ticketRepository.GetByIdForReviewAsync(@event.TicketId);
        if (ticket is not null)
            ticket.MarkReviewed();
        else
            logger.TicketNotFoundForReviewEvent(@event.TicketId);

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
