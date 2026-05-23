using Concertable.Kernel;

namespace Concertable.Customer.Review.Domain.Events;

public record ReviewCreatedDomainEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars) : IDomainEvent;
