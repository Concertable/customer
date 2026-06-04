using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;

namespace Concertable.Customer.Review.Application.Interfaces;

internal interface IConcertReviewRepository
{
    Task<IPagination<ReviewDto>> GetByConcertAsync(int concertId, IPageParams pageParams);
    Task<ReviewSummary> GetSummaryByConcertAsync(int concertId);
    Task<bool> HasReviewForTicketAsync(Guid ticketId);
    Task<ReviewEntity> AddAsync(ReviewEntity review);
    Task SaveChangesAsync();
}
