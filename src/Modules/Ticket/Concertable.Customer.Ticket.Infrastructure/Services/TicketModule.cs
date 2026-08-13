using Concertable.Customer.Ticket.Contracts;
using Reunion;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class TicketModule : ITicketModule
{
    private readonly ITicketRepository ticketRepository;

    public TicketModule(ITicketRepository ticketRepository)
    {
        this.ticketRepository = ticketRepository;
    }

    public Task<Option<TicketSummary>> GetByUserAndConcertAsync(Guid userId, int concertId) =>
        ticketRepository.GetSummaryByUserAndConcertAsync(userId, concertId).ToOption();

    public Task<bool> CanReviewArtistAsync(Guid userId, int artistId) =>
        ticketRepository.CanReviewArtistAsync(userId, artistId);

    public Task<bool> CanReviewVenueAsync(Guid userId, int venueId) =>
        ticketRepository.CanReviewVenueAsync(userId, venueId);
}
