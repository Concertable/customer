using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Mappers;
using Concertable.Customer.Ticket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Repositories;

internal class ArtistReviewRepository(
    ReviewDbContext context,
    ITicketRepository ticketRepository,
    TimeProvider timeProvider) : IArtistReviewRepository
{
    public Task<IPagination<ReviewDto>> GetByArtistAsync(int artistId, IPageParams pageParams) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.ArtistId == artistId)
            .OrderByDescending(r => r.Id)
            .ToDto()
            .ToPaginationAsync(pageParams);

    // BROKEN Phase 1: needs to ask "did userId buy a ticket for any concert featuring artistId, that's started,
    // and not yet reviewed?" — that's a Customer.Ticket query keyed on artistId. ITicketRepository today exposes
    // only by-concertId. Returns false until Customer.Ticket grows the right read or this moves to a join over
    // the projection that B2B publishes.
    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        Task.FromResult(false);
}
