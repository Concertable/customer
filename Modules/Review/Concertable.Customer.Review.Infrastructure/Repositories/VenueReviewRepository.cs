using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal class VenueReviewRepository(ReviewDbContext context) : IVenueReviewRepository
{
    public Task<IPagination<ReviewDto>> GetByVenueAsync(int venueId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.VenueId == venueId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    // BROKEN Phase 1: same shape as ArtistReviewRepository.CanUserReviewArtistAsync. Returns false until the
    // ticket-by-venue read exists.
    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        Task.FromResult(false);
}
