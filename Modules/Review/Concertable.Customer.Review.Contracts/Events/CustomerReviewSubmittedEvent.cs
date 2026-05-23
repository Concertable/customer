using Concertable.Messaging;

namespace Concertable.Customer.Review.Contracts.Events;

public record CustomerReviewSubmittedEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars) : IIntegrationEvent;
