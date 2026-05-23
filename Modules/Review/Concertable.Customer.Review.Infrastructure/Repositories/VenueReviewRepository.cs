using Concertable.Contracts;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Concertable.Customer.Ticket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal class VenueReviewRepository : IVenueReviewRepository
{
    private readonly ReviewDbContext context;
    private readonly ITicketRepository ticketRepository;

    public VenueReviewRepository(ReviewDbContext context, ITicketRepository ticketRepository)
    {
        this.context = context;
        this.ticketRepository = ticketRepository;
    }

    public Task<IPagination<ReviewDto>> GetByVenueAsync(int venueId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.VenueId == venueId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        ticketRepository.CanReviewVenueAsync(userId, venueId);
}
