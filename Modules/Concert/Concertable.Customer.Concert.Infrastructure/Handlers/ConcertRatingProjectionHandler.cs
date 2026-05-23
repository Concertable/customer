using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal class ConcertRatingProjectionHandler : IIntegrationEventHandler<ConcertRatingUpdatedEvent>
{
    private readonly ConcertDbContext context;

    public ConcertRatingProjectionHandler(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(ConcertRatingUpdatedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ConcertRatingProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(ConcertRatingProjectionHandler));

        var concert = await context.Concerts.FirstOrDefaultAsync(c => c.Id == e.ConcertId, ct);
        if (concert is null)
            return;

        concert.UpdateRating(e.AverageRating, e.ReviewCount);

        await context.SaveChangesAsync(ct);
    }
}
