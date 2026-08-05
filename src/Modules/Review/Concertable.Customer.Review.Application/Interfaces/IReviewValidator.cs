using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Functional;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IReviewValidator
{
    Task<Result<TicketSummary, CreateReviewError>> GetReviewableTicketAsync(Guid userId, int concertId);
    Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
}
