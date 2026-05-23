using Concertable.Customer.Ticket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Ticket.Infrastructure.Repositories;

internal class TicketRepository : GuidRepository<TicketEntity>, ITicketRepository
{
    private readonly TimeProvider timeProvider;

    public TicketRepository(TicketDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public Task<byte[]?> GetQrCodeByIdAsync(Guid id)
    {
        return context.Tickets
            .Where(t => t.Id == id)
            .Select(t => t.QrCode)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TicketEntity>> GetHistoryByUserIdAsync(Guid id)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await context.Tickets
            .Where(t => t.UserId == id && t.Period.Start < now)
            .ToListAsync();
    }

    public async Task<IEnumerable<TicketEntity>> GetUpcomingByUserIdAsync(Guid id)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await context.Tickets
            .Where(t => t.UserId == id && t.Period.Start >= now)
            .ToListAsync();
    }

    public Task<TicketEntity?> GetByUserIdAndConcertIdAsync(Guid userId, int concertId) =>
        context.Tickets.FirstOrDefaultAsync(t => t.UserId == userId && t.ConcertId == concertId);

    public Task<TicketEntity?> GetByIdForReviewAsync(Guid ticketId) =>
        context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

    public Task<bool> CanReviewArtistAsync(Guid userId, int artistId)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return context.Tickets.AnyAsync(t =>
            t.UserId == userId &&
            t.ArtistId == artistId &&
            !t.HasReview &&
            t.Period.Start <= now);
    }

    public Task<bool> CanReviewVenueAsync(Guid userId, int venueId)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return context.Tickets.AnyAsync(t =>
            t.UserId == userId &&
            t.VenueId == venueId &&
            !t.HasReview &&
            t.Period.Start <= now);
    }
}
