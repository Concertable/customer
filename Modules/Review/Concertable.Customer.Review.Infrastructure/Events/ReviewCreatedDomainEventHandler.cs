using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Review.Domain.Events;
using Concertable.Kernel;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Review.Infrastructure.Events;

internal sealed class ReviewCreatedDomainEventHandler : IPreCommitDomainEventHandler<ReviewCreatedDomainEvent>
{
    private readonly IBus bus;

    public ReviewCreatedDomainEventHandler(IBus bus)
    {
        this.bus = bus;
    }

    public Task HandleAsync(ReviewCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new CustomerReviewSubmittedEvent(e.TicketId, e.ArtistId, e.VenueId, e.ConcertId, e.Stars, e.Email, e.Details), ct);
}
