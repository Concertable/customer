using Concertable.Customer.Ticket.Application.Interfaces;
using Concertable.Customer.Ticket.Contracts;
using Reunion;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class TicketModule : ITicketModule
{
    private readonly ITicketService ticketService;

    public TicketModule(ITicketService ticketService)
    {
        this.ticketService = ticketService;
    }

    public Task<Option<TicketSummary>> GetByUserAndConcertAsync(Guid userId, int concertId) =>
        ticketService.GetByUserAndConcertAsync(userId, concertId);

    public Task<bool> CanReviewArtistAsync(Guid userId, int artistId) =>
        ticketService.CanReviewArtistAsync(userId, artistId);

    public Task<bool> CanReviewVenueAsync(Guid userId, int venueId) =>
        ticketService.CanReviewVenueAsync(userId, venueId);
}
