using Concertable.Contracts;
using Concertable.Kernel.Identity;

namespace Concertable.Customer.Review.Infrastructure.Services;

internal sealed class ArtistReviewService : IArtistReviewService
{
    private readonly IArtistReviewRepository reviewRepository;
    private readonly IReviewValidator reviewValidator;
    private readonly ICurrentUser currentUser;

    public ArtistReviewService(
        IArtistReviewRepository reviewRepository,
        IReviewValidator reviewValidator,
        ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.reviewValidator = reviewValidator;
        this.currentUser = currentUser;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams) =>
        reviewRepository.GetByArtistAsync(artistId, pageParams);

    public Task<ReviewSummary> GetSummaryAsync(int artistId) =>
        reviewRepository.GetSummaryByArtistAsync(artistId);

    public async Task<bool> CanCurrentUserReviewAsync(int artistId) =>
        currentUser.IsAuthenticated
        && (await reviewValidator.ValidateArtistAsync(currentUser.GetId(), artistId)).IsValid;
}
