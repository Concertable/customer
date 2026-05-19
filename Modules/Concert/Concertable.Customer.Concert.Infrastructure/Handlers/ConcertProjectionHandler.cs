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
            concert = ConcertEntity.Create(e.ConcertId, e.TotalTickets, e.Price, e.Period, e.DatePosted);
            await repository.AddAsync(concert);
        }
        else
        {
            concert.Update(e.TotalTickets, e.Price, e.Period, e.DatePosted);
        }

        await repository.SaveChangesAsync();
    }
}
