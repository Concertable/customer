using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Application.Responses;
using Concertable.Payment.Domain;
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
