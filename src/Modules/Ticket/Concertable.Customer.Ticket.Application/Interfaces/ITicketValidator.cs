using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Errors;
using Reunion;
using Reunion.Validation;

namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketValidator
{
    ValidationResult CanBePurchased(ConcertDto concert);
    Task<Result<ValidationResult, EligibilityError>> CanBePurchasedAsync(int concertId);
    ValidationResult CanPurchaseTickets(ConcertDto concert, int quantity);
}
