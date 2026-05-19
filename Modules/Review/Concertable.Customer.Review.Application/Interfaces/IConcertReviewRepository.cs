namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IConcertReviewRepository
{
    Task<IPagination<ReviewDto>> GetByConcertAsync(int concertId, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryByConcertAsync(int concertId);
    Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId);
    Task<ReviewEntity> AddAsync(ReviewEntity review);
    Task SaveChangesAsync();
}
