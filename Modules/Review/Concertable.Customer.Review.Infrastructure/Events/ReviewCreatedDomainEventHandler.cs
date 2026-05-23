using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Review.Domain.Events;

namespace Concertable.Customer.Review.Infrastructure.Events;

internal class ReviewCreatedDomainEventHandler(IBus bus)
    : IPreCommitDomainEventHandler<ReviewCreatedDomainEvent>
{
    public Task HandleAsync(ReviewCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new CustomerReviewSubmittedEvent(e.TicketId, e.ArtistId, e.VenueId, e.ConcertId, e.Stars), ct);
}
