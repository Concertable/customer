using Concertable.Contracts;
using Concertable.Kernel.Identity;

namespace Concertable.Customer.Review.Infrastructure.Services;

internal sealed class VenueReviewService : IVenueReviewService
{
    private readonly IVenueReviewRepository reviewRepository;
    private readonly IReviewValidator reviewValidator;
    private readonly ICurrentUser currentUser;

    public VenueReviewService(
        IVenueReviewRepository reviewRepository,
        IReviewValidator reviewValidator,
        ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.reviewValidator = reviewValidator;
        this.currentUser = currentUser;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int venueId, IPageParams pageParams) =>
        reviewRepository.GetByVenueAsync(venueId, pageParams);

    public Task<bool> CanCurrentUserReviewAsync(int venueId) =>
        reviewValidator.CanUserReviewVenueAsync(currentUser.GetId(), venueId);
}
