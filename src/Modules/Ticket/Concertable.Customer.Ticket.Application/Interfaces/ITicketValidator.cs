using Concertable.Customer.Concert.Contracts;
using FluentResults;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketValidator
{
    Result CanBePurchased(ConcertDto concert);
    Task<Result> CanBePurchasedAsync(int concertId);
    Result CanPurchaseTickets(ConcertDto concert, int quantity);
}
