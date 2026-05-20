using Concertable.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Messaging.Domain;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal class ConcertProjectionHandler : IIntegrationEventHandler<ConcertChangedEvent>
{
    private readonly IConcertRepository repository;
    private readonly ConcertDbContext context;

    public ConcertProjectionHandler(IConcertRepository repository, ConcertDbContext context)
    {
        this.repository = repository;
        this.context = context;
    }

    public async Task HandleAsync(ConcertChangedEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        if (await context.Set<InboxMessageEntity>().AnyAsync(
            m => m.MessageId == envelope.MessageId && m.ConsumerName == nameof(ConcertProjectionHandler), ct))
            return;

        context.Set<InboxMessageEntity>().Add(
            InboxMessageEntity.Create(envelope.MessageId, nameof(ConcertProjectionHandler), envelope.MessageType, DateTimeOffset.UtcNow));

        var concert = await repository.GetByIdAsync(e.ConcertId);

        if (concert is null)
        {
            concert = ConcertEntity.Create(
                e.ConcertId,
                e.Name,
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
            await repository.AddAsync(concert);
        }
        else
        {
            concert.Update(
                e.Name,
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
        }

        await repository.SaveChangesAsync();
    }
}
