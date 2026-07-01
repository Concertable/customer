using Concertable.Customer.Ticket.Contracts;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class TicketModule : ITicketModule
{
    private readonly ITicketRepository ticketRepository;

    public TicketModule(ITicketRepository ticketRepository)
    {
        this.ticketRepository = ticketRepository;
    }

    public Task<TicketSummary?> GetByUserAndConcertAsync(Guid userId, int concertId) =>
        ticketRepository.GetSummaryByUserAndConcertAsync(userId, concertId);

    public Task<bool> CanReviewArtistAsync(Guid userId, int artistId) =>
        ticketRepository.CanReviewArtistAsync(userId, artistId);

    public Task<bool> CanReviewVenueAsync(Guid userId, int venueId) =>
        ticketRepository.CanReviewVenueAsync(userId, venueId);
}
