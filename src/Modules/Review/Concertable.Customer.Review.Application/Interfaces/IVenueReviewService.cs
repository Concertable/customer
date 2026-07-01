using Concertable.Contracts;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IVenueReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int venueId, IPageParams pageParams);
    Task<bool> CanCurrentUserReviewAsync(int venueId);
}
