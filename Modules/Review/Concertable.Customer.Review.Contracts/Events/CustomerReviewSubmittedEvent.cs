using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Review.Contracts.Events;

public record CustomerReviewSubmittedEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars,
    string Email,
    string? Details) : IIntegrationEvent;
