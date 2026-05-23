using Concertable.Customer.Concert.Domain;
using FluentResults;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketValidator
{
    Result CanBePurchased(ConcertReadModel concert);
    Task<Result> CanBePurchasedAsync(int concertId);
    Result CanPurchaseTickets(ConcertReadModel concert, int quantity);
}
