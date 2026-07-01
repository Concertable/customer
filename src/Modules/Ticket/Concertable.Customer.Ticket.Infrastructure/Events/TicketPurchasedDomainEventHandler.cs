using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Customer.Ticket.Domain.Events;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Ticket.Infrastructure.Events;

internal sealed class TicketPurchasedDomainEventHandler : IPreCommitDomainEventHandler<TicketPurchasedDomainEvent>
{
    private readonly IBus bus;

    public TicketPurchasedDomainEventHandler(IBus bus)
    {
        this.bus = bus;
    }

    public Task HandleAsync(TicketPurchasedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new TicketPurchasedEvent(e.TicketId, e.UserId, e.ConcertId, e.Price, e.PurchaseDate), ct);
}
