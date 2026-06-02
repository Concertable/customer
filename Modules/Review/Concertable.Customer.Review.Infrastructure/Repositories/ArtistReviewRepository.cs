using Concertable.Contracts;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Concertable.Customer.Ticket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal sealed class ArtistReviewRepository : IArtistReviewRepository
{
    private readonly ReviewDbContext context;
    private readonly ITicketRepository ticketRepository;

    public ArtistReviewRepository(ReviewDbContext context, ITicketRepository ticketRepository)
    {
        this.context = context;
        this.ticketRepository = ticketRepository;
    }

    public Task<IPagination<ReviewDto>> GetByArtistAsync(int artistId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.ArtistId == artistId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public async Task<ReviewSummary> GetSummaryByArtistAsync(int artistId)
    {
        var rows = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ArtistId == artistId)
            .Select(r => (double)r.Stars)
            .ToListAsync();

        if (rows.Count == 0)
            return new ReviewSummary(0, null);

        return new ReviewSummary(rows.Count, Math.Round(rows.Average(), 1));
    }

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        ticketRepository.CanReviewArtistAsync(userId, artistId);
}
