using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.B2B.Venue.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Handlers;

internal sealed class VenueRatingProjectionHandler : IIntegrationEventHandler<VenueRatingUpdatedEvent>
{
    private readonly VenueDbContext context;

    public VenueRatingProjectionHandler(VenueDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(VenueRatingUpdatedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(VenueRatingProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(VenueRatingProjectionHandler));

        var venue = await context.Venues.FirstOrDefaultAsync(v => v.Id == e.VenueId, ct);
        if (venue is null)
            return;

        venue.UpdateRating(e.AverageRating, e.ReviewCount);

        await context.SaveChangesAsync(ct);
    }
}
