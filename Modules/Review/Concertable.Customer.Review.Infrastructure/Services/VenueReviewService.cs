namespace Concertable.Customer.Review.Infrastructure.Services;

internal class VenueReviewService(
    IVenueReviewRepository reviewRepository,
    IReviewValidator reviewValidator,
    ICurrentUser currentUser) : IVenueReviewService
{
    public Task<IPagination<ReviewDto>> GetAsync(int venueId, IPageParams pageParams) =>
        reviewRepository.GetByVenueAsync(venueId, pageParams);

    public Task<bool> CanCurrentUserReviewAsync(int venueId) =>
        reviewValidator.CanUserReviewVenueAsync(currentUser.GetId(), venueId);
}
