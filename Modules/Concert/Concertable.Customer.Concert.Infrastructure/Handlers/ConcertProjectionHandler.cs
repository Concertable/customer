using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal class ConcertProjectionHandler : IIntegrationEventHandler<ConcertChangedEvent>
{
    private readonly ConcertDbContext context;

    public ConcertProjectionHandler(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(ConcertChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(ConcertProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(ConcertProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var concert = await context.Concerts
            .Include(c => c.Genres)
            .FirstOrDefaultAsync(c => c.Id == e.ConcertId, ct);

        if (concert is null)
        {
            concert = ConcertReadModel.Create(
                e.ConcertId,
                e.Name,
                e.About,
                e.BannerUrl,
                e.Avatar,
                e.TotalTickets,
                e.Price,
                e.Period,
                e.DatePosted,
                e.ArtistId,
                e.ArtistName,
                e.VenueId,
                e.VenueName,
                e.PayeeUserId,
                e.ContractType);

            foreach (var g in e.Genres)
                concert.Genres.Add(new ConcertGenreReadModel { ConcertId = e.ConcertId, Genre = g });

            context.Concerts.Add(concert);
        }
        else
        {
            concert.Update(
                e.Name,
                e.About,
                e.BannerUrl,
                e.Avatar,
                e.TotalTickets,
                e.Price,
                e.Period,
                e.DatePosted,
                e.ArtistId,
                e.ArtistName,
                e.VenueId,
                e.VenueName,
                e.PayeeUserId,
                e.ContractType);

            var desired = e.Genres.ToHashSet();
            var current = concert.Genres.Select(g => g.Genre).ToHashSet();

            foreach (var g in concert.Genres.Where(g => !desired.Contains(g.Genre)).ToList())
                concert.Genres.Remove(g);
            foreach (var g in desired.Where(g => !current.Contains(g)))
                concert.Genres.Add(new ConcertGenreReadModel { ConcertId = e.ConcertId, Genre = g });
        }

        await context.SaveChangesAsync(ct);
    }
}
