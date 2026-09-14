using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal sealed class ArtistReadModelProjectionHandler : IIntegrationEventHandler<ArtistChangedEvent>
{
    private readonly ConcertDbContext context;

    public ArtistReadModelProjectionHandler(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(ArtistChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(ArtistReadModelProjectionHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(ArtistReadModelProjectionHandler));

        var artist = await context.ArtistReadModels
            .Include(a => a.Genres)
            .FirstOrDefaultAsync(a => a.Id == e.ArtistId, ct);

        if (artist is null)
        {
            artist = new ArtistReadModel
            {
                Id = e.ArtistId,
                Name = e.Name,
                Avatar = e.Avatar,
                Address = new Address(e.County, e.Town),
                Genres = e.Genres
                    .Select(g => new ArtistReadModelGenre { ArtistReadModelId = e.ArtistId, Genre = g })
                    .ToList()
            };
            context.ArtistReadModels.Add(artist);
        }
        else
        {
            artist.Name = e.Name;
            artist.Avatar = e.Avatar;
            artist.Address = new Address(e.County, e.Town);

            var desired = e.Genres.ToHashSet();
            var current = artist.Genres.Select(g => g.Genre).ToHashSet();

            foreach (var g in artist.Genres.Where(g => !desired.Contains(g.Genre)).ToList())
                artist.Genres.Remove(g);
            foreach (var g in desired.Where(g => !current.Contains(g)))
                artist.Genres.Add(new ArtistReadModelGenre { ArtistReadModelId = e.ArtistId, Genre = g });
        }

        await context.SaveChangesAsync(ct);
    }
}
