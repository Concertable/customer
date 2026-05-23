namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketRepository : IGuidRepository<TicketEntity>
{
    Task<byte[]?> GetQrCodeByIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(Guid id);
    Task<TicketEntity?> GetByUserIdAndConcertIdAsync(Guid userId, int concertId);
    Task<TicketEntity?> GetByIdForReviewAsync(Guid ticketId);
    Task<bool> CanReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanReviewVenueAsync(Guid userId, int venueId);
}
