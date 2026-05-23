using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Messaging.Domain;
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
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(ArtistRatingProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(ArtistRatingProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var artist = await context.Artists.FirstOrDefaultAsync(a => a.Id == e.ArtistId, ct);
        if (artist is null)
            return;

        artist.UpdateRating(e.AverageRating, e.ReviewCount);

        await context.SaveChangesAsync(ct);
    }
}
