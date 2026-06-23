using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal sealed class ArtistRatingReadModelProjectionHandler : IIntegrationEventHandler<ArtistRatingUpdatedEvent>
{
    private readonly ConcertDbContext context;

    public ArtistRatingReadModelProjectionHandler(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(ArtistRatingUpdatedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ArtistRatingReadModelProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(ArtistRatingReadModelProjectionHandler));

        var artist = await context.ArtistReadModels.FirstOrDefaultAsync(a => a.Id == e.ArtistId, ct);
        if (artist is null)
            return;

        artist.AverageRating = e.AverageRating;
        artist.ReviewCount = e.ReviewCount;

        await context.SaveChangesAsync(ct);
    }
}
