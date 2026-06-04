namespace Concertable.Customer.Ticket.Contracts;

public interface ITicketModule
{
    Task<TicketSummary?> GetByUserAndConcertAsync(Guid userId, int concertId);
    Task<bool> CanReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanReviewVenueAsync(Guid userId, int venueId);
}

public sealed record TicketSummary(
    Guid Id,
    int ConcertId,
    int ArtistId,
    int VenueId,
    DateTime PeriodStart);
