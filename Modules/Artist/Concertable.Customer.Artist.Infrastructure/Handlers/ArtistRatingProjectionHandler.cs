using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Customer.Artist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Handlers;

internal class ArtistRatingProjectionHandler : IIntegrationEventHandler<ArtistRatingUpdatedEvent>
{
    private readonly ArtistDbContext context;

    public ArtistRatingProjectionHandler(ArtistDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(ArtistRatingUpdatedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ArtistRatingProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(ArtistRatingProjectionHandler));

        var artist = await context.Artists.FirstOrDefaultAsync(a => a.Id == e.ArtistId, ct);
        if (artist is null)
            return;

        artist.UpdateRating(e.AverageRating, e.ReviewCount);

        await context.SaveChangesAsync(ct);
    }
}
