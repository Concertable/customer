using Concertable.Kernel;

namespace Concertable.Customer.Review.Domain.Events;

public sealed record ReviewCreatedDomainEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars,
    string Email,
    string? Details) : IDomainEvent;
