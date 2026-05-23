using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Messaging.Domain;
using Concertable.Venue.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Handlers;

internal class VenueRatingProjectionHandler : IIntegrationEventHandler<VenueRatingUpdatedEvent>
{
    private readonly VenueDbContext context;

    public VenueRatingProjectionHandler(VenueDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(VenueRatingUpdatedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(VenueRatingProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(VenueRatingProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var venue = await context.Venues.FirstOrDefaultAsync(v => v.Id == e.VenueId, ct);
        if (venue is null)
            return;

        venue.UpdateRating(e.AverageRating, e.ReviewCount);

        await context.SaveChangesAsync(ct);
    }
}
