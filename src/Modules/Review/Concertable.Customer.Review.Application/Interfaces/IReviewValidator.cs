using Concertable.Customer.Ticket.Contracts;
using Reunion.Validation;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IReviewValidator
{
    ValidationResult ValidateReviewPeriod(TicketSummary ticket);
    Task<ValidationResult> ValidateTicketNotReviewedAsync(Guid ticketId);
    Task<ValidationResult> ValidateArtistAsync(Guid userId, int artistId);
    Task<ValidationResult> ValidateVenueAsync(Guid userId, int venueId);
}
