using Concertable.Contracts;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal sealed class VenueReviewRepository : IVenueReviewRepository
{
    private readonly ReviewDbContext context;

    public VenueReviewRepository(ReviewDbContext context)
    {
        this.context = context;
    }

    public Task<IPagination<ReviewDto>> GetByVenueAsync(int venueId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.VenueId == venueId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);
}
