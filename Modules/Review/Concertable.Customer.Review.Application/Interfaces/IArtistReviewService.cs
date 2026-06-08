using Concertable.Contracts;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IArtistReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams);
    Task<ReviewSummary> GetSummaryAsync(int artistId);
    Task<bool> CanCurrentUserReviewAsync(int artistId);
}
