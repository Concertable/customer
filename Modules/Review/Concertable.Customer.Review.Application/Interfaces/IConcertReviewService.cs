using Concertable.Contracts;
using Concertable.Customer.Review.Application.Requests;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IConcertReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams);
    Task<ReviewSummaryDto> GetSummaryAsync(int concertId);
    Task<bool> CanCurrentUserReviewAsync(int concertId);
    Task<ReviewDto> CreateAsync(CreateReviewRequest request);
}
