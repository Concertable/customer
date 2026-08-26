using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Contracts;
using Reunion;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketService
{
    Task<Result<TicketPayment, PurchaseError>> PurchaseAsync(TicketPurchaseParams purchaseParams);
    Task<TicketPayment> CompleteAsync(PurchaseComplete purchaseCompleteDto);
    Task<Result<TicketCheckout, CheckoutError>> CheckoutAsync(int concertId, int quantity);
    Task<IEnumerable<TicketDto>> GetUserUpcomingAsync();
    Task<IEnumerable<TicketDto>> GetUserHistoryAsync();
    Task<Option<TicketSummary>> GetByUserAndConcertAsync(Guid userId, int concertId);
    Task<bool> CanReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanReviewVenueAsync(Guid userId, int venueId);
}
