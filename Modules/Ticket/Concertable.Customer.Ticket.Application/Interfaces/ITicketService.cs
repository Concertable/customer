using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Application.Responses;
using FluentResults;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketService
{
    Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams);
    Task<Result<TicketPaymentResponse>> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    Task<Result<TicketCheckout>> CheckoutAsync(int concertId, int quantity);
    Task<IEnumerable<TicketDto>> GetUserUpcomingAsync();
    Task<IEnumerable<TicketDto>> GetUserHistoryAsync();
}
