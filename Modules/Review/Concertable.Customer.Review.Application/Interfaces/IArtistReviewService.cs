using Concertable.Contracts;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IArtistReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams);
    Task<bool> CanCurrentUserReviewAsync(int artistId);
}
