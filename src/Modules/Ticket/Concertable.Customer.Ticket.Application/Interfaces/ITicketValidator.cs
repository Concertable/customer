using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Errors;
using Reunion;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketValidator
{
    bool CanBePurchased(ConcertDto concert);
    Task<Result<bool, EligibilityError>> CanBePurchasedAsync(int concertId);
    UnitResult<IReadOnlyList<string>> CanPurchaseTickets(ConcertDto concert, int quantity);
}
