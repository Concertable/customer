using Concertable.Contracts;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IArtistReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int artistId, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int artistId);
    Task<bool> CanCurrentUserReviewAsync(int artistId);
}
