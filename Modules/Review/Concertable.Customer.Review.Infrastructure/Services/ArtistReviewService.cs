namespace Concertable.Customer.Review.Infrastructure.Services;

internal class ArtistReviewService(
    IArtistReviewRepository reviewRepository,
    IReviewValidator reviewValidator,
    ICurrentUser currentUser) : IArtistReviewService
{
    public Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams) =>
        reviewRepository.GetByArtistAsync(artistId, pageParams);

    public Task<bool> CanCurrentUserReviewAsync(int artistId) =>
        reviewValidator.CanUserReviewArtistAsync(currentUser.GetId(), artistId);
}
