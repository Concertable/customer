using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Review.Contracts.Events;

[MessageType("concertable.customer.customer-review-submitted.v1")]
public sealed record CustomerReviewSubmittedEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars,
    string Email,
    string? Details) : IIntegrationEvent;
