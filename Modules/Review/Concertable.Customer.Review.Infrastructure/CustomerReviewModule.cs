using Concertable.Customer.Review.Contracts;

namespace Concertable.Customer.Review.Infrastructure;

internal sealed class CustomerReviewModule(
    IArtistReviewRepository artistReviewRepository,
    IVenueReviewRepository venueReviewRepository,
    IReviewValidator reviewValidator) : ICustomerReviewModule
{
    public Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams) =>
        artistReviewRepository.GetByArtistAsync(artistId, pageParams);

    public Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams) =>
        venueReviewRepository.GetByVenueAsync(venueId, pageParams);

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        reviewValidator.CanUserReviewArtistAsync(userId, artistId);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        reviewValidator.CanUserReviewVenueAsync(userId, venueId);
}
