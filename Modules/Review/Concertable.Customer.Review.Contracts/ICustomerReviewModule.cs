using Concertable.Shared;

namespace Concertable.Customer.Review.Contracts;

public interface ICustomerReviewModule
{
    Task<IPagination<ReviewDto>> GetReviewsByArtistAsync(int artistId, IPageParams pageParams);
    Task<IPagination<ReviewDto>> GetReviewsByVenueAsync(int venueId, IPageParams pageParams);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
    Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId);
}
