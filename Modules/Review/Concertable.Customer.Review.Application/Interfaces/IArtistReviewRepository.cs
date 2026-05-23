using Concertable.Contracts;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IArtistReviewRepository
{
    Task<IPagination<ReviewDto>> GetByArtistAsync(int artistId, IPageParams pageParams);
    Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId);
}
