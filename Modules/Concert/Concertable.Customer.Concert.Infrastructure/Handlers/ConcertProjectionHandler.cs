using Concertable.Concert.Contracts.Events;

namespace Concertable.Customer.Concert.Infrastructure.Handlers;

internal class ConcertProjectionHandler(IConcertRepository repository)
    : IIntegrationEventHandler<ConcertChangedEvent>
{
    public async Task HandleAsync(ConcertChangedEvent e, CancellationToken ct = default)
    {
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
