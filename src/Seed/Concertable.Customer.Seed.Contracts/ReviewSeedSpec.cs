namespace Concertable.Customer.Seed.Contracts;

public sealed record ReviewSeedSpec(
    Guid MessageId,
    DateTimeOffset OccurredAtUtc,
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    byte Stars,
    string Email,
    string? Details);
