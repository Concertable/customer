using Concertable.Concert.Contracts.Events;
using Concertable.Customer.Review.Domain.Events;

namespace Concertable.Customer.Review.Infrastructure.Events;

internal class ReviewCreatedDomainEventHandler(IBus bus)
    : IDomainEventHandler<ReviewCreatedDomainEvent>
{
    public Task HandleAsync(ReviewCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new ReviewSubmittedEvent(e.ArtistId, e.VenueId, e.ConcertId, e.Stars), ct);
}
