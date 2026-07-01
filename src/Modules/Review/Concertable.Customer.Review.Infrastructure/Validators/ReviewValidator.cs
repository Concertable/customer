using Concertable.Customer.Ticket.Contracts;

namespace Concertable.Customer.Review.Infrastructure.Validators;

internal sealed class ReviewValidator : IReviewValidator
{
    private readonly IConcertReviewRepository concertReviewRepository;
    private readonly ITicketModule ticketModule;
    private readonly TimeProvider timeProvider;

    public ReviewValidator(
        IConcertReviewRepository concertReviewRepository,
        ITicketModule ticketModule,
        TimeProvider timeProvider)
    {
        this.concertReviewRepository = concertReviewRepository;
        this.ticketModule = ticketModule;
        this.timeProvider = timeProvider;
    }

    public async Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId)
    {
        var ticket = await ticketModule.GetByUserAndConcertAsync(userId, concertId);
        if (ticket is null || ticket.PeriodStart > timeProvider.GetUtcNow())
            return false;

        return !await concertReviewRepository.HasReviewForTicketAsync(ticket.Id);
    }

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        ticketModule.CanReviewArtistAsync(userId, artistId);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        ticketModule.CanReviewVenueAsync(userId, venueId);
}
