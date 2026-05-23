using Concertable.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Messaging.Domain;
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
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(ConcertRatingProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(ConcertRatingProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var concert = await context.Concerts.FirstOrDefaultAsync(c => c.Id == e.ConcertId, ct);
        if (concert is null)
            return;

        concert.UpdateRating(e.AverageRating, e.ReviewCount);

        await context.SaveChangesAsync(ct);
    }
}
