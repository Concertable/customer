namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IVenueReviewRepository
{
    Task<IPagination<ReviewDto>> GetByVenueAsync(int venueId, IPageParams pageParams);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
}
