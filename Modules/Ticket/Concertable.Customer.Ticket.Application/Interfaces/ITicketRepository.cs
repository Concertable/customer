using Concertable.Customer.Ticket.Contracts;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketRepository : IRepository<TicketEntity, Guid>
{
    Task<byte[]?> GetQrCodeByIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(Guid id);
    Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(Guid id);
    Task<TicketSummary?> GetSummaryByUserAndConcertAsync(Guid userId, int concertId);
    Task<TicketEntity?> GetByIdForReviewAsync(Guid ticketId);
    Task<bool> CanReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanReviewVenueAsync(Guid userId, int venueId);
}
