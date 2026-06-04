using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using FluentResults;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketService
{
    Task<Result<TicketPayment>> PurchaseAsync(TicketPurchaseParams purchaseParams);
    Task<Result<TicketPayment>> CompleteAsync(PurchaseComplete purchaseCompleteDto);
    Task<Result<TicketCheckout>> CheckoutAsync(int concertId, int quantity);
    Task<IEnumerable<TicketDto>> GetUserUpcomingAsync();
    Task<IEnumerable<TicketDto>> GetUserHistoryAsync();
}
