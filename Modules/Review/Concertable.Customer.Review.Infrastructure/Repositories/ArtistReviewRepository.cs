using Concertable.Contracts;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal sealed class ArtistReviewRepository : IArtistReviewRepository
{
    private readonly ReviewDbContext context;

    public ArtistReviewRepository(ReviewDbContext context)
    {
        this.context = context;
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
}
