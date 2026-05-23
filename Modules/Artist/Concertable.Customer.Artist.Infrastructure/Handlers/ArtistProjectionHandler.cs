using Concertable.Artist.Contracts.Events;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Handlers;

internal class ArtistProjectionHandler : IIntegrationEventHandler<ArtistChangedEvent>
{
    private readonly ArtistDbContext context;

    public ArtistProjectionHandler(ArtistDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(ArtistChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(ArtistProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(ArtistProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var artist = await context.Artists
            .Include(a => a.Genres)
            .FirstOrDefaultAsync(a => a.Id == e.ArtistId, ct);

        if (artist is null)
        {
            artist = ArtistEntity.Create(
                e.ArtistId,
                e.UserId,
                e.Name,
                e.About,
                e.Avatar,
                e.BannerUrl,
                e.County,
                e.Town,
                e.Latitude,
                e.Longitude,
                e.Email);

            foreach (var g in e.Genres)
                artist.Genres.Add(new ArtistGenreEntity { ArtistId = e.ArtistId, Genre = g });

            context.Artists.Add(artist);
        }
        else
        {
            artist.Update(
                e.UserId,
                e.Name,
                e.About,
                e.Avatar,
                e.BannerUrl,
                e.County,
                e.Town,
                e.Latitude,
                e.Longitude,
                e.Email);

            var desired = e.Genres.ToHashSet();
            var current = artist.Genres.Select(g => g.Genre).ToHashSet();

            foreach (var g in artist.Genres.Where(g => !desired.Contains(g.Genre)).ToList())
                artist.Genres.Remove(g);
            foreach (var g in desired.Where(g => !current.Contains(g)))
                artist.Genres.Add(new ArtistGenreEntity { ArtistId = e.ArtistId, Genre = g });
        }

        await context.SaveChangesAsync(ct);
    }
}
