using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Concertable.Customer.Ticket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal sealed class ConcertReviewRepository(
    ReviewDbContext context,
    ITicketRepository ticketRepository,
    TimeProvider timeProvider) : IConcertReviewRepository
{
    public Task<IPagination<ReviewDto>> GetByConcertAsync(int concertId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.ConcertId == concertId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummaryDto> GetSummaryByConcertAsync(int concertId)
    {
        var rows = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ConcertId == concertId)
            .Select(r => (double)r.Stars)
            .ToListAsync();

        if (rows.Count == 0)
            return new ReviewSummaryDto(0, null);

        return new ReviewSummaryDto(rows.Count, Math.Round(rows.Average(), 1));
    }

    public async Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId)
    {
        var ticket = await ticketRepository.GetByUserIdAndConcertIdAsync(userId, concertId);
        if (ticket is null || ticket.Period.Start > timeProvider.GetUtcNow())
            return false;

        return !await context.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.TicketId == ticket.Id);
    }

    public async Task<ReviewEntity> AddAsync(ReviewEntity review)
    {
        await context.Reviews.AddAsync(review);
        return review;
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
