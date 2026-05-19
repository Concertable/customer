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

    public Task<TicketEntity?> GetByUserIdAndConcertIdAsync(Guid userId, int concertId)
    {
        return context.Tickets
            .FirstOrDefaultAsync(t => t.UserId == userId && t.ConcertId == concertId);
    }
}
