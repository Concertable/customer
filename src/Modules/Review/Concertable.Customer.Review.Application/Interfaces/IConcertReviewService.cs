using Concertable.Contracts;
using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Review.Application.Requests;
using Reunion;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IConcertReviewService
{
    Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams);
    Task<ReviewSummary> GetSummaryAsync(int concertId);
    Task<bool> CanCurrentUserReviewAsync(int concertId);
    Task<Result<ReviewDto, CreateReviewError>> CreateAsync(int concertId, CreateReviewRequest request);
}
