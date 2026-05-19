using Concertable.Customer.Concert.Domain;
using FluentResults;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketValidator
{
    Result CanBePurchased(ConcertEntity concert);
    Task<Result> CanBePurchasedAsync(int concertId);
    Result CanPurchaseTickets(ConcertEntity concert, int quantity);
}
