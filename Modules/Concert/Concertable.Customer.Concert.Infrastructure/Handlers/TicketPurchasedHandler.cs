using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal sealed class TicketPurchasedHandler : IIntegrationEventHandler<TicketPurchasedEvent>
{
    private readonly ConcertDbContext context;

    public TicketPurchasedHandler(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(TicketPurchasedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(TicketPurchasedHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(TicketPurchasedHandler));

        var concert = await context.Concerts.FirstOrDefaultAsync(c => c.Id == e.ConcertId, ct);
        if (concert is null)
            return;

        concert.DecrementAvailability(1);

        await context.SaveChangesAsync(ct);
    }
}
