using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal sealed class ConcertReviewRepository : IConcertReviewRepository
{
    private readonly ReviewDbContext context;

    public ConcertReviewRepository(ReviewDbContext context)
    {
        this.context = context;
    }

    public Task<IPagination<ReviewDto>> GetByConcertAsync(int concertId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.ConcertId == concertId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummary> GetSummaryByConcertAsync(int concertId)
    {
        var rows = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ConcertId == concertId)
            .Select(r => (double)r.Stars)
            .ToListAsync();

        if (rows.Count == 0)
            return new ReviewSummary(0, null);

        return new ReviewSummary(rows.Count, Math.Round(rows.Average(), 1));
    }

    public Task<bool> HasReviewForTicketAsync(Guid ticketId) =>
        context.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.TicketId == ticketId);

    public async Task<ReviewEntity> AddAsync(ReviewEntity review)
    {
        await context.Reviews.AddAsync(review);
        return review;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
