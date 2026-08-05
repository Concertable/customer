using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Functional;

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

    public async Task<Result<TicketSummary, CreateReviewError>> GetReviewableTicketAsync(
        Guid userId,
        int concertId)
    {
        var ticket = await ticketModule.GetByUserAndConcertAsync(userId, concertId);
        if (ticket is null)
            return Result.Failure<TicketSummary, CreateReviewError>(CreateReviewError.TicketNotFound);

        if (ticket.PeriodStart > timeProvider.GetUtcNow())
            return Result.Failure<TicketSummary, CreateReviewError>(CreateReviewError.ConcertNotReviewableYet);

        if (await concertReviewRepository.HasReviewForTicketAsync(ticket.Id))
            return Result.Failure<TicketSummary, CreateReviewError>(CreateReviewError.ReviewAlreadyExists);

        return Result.Success<TicketSummary, CreateReviewError>(ticket);
    }

    public async Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId)
    {
        var result = await GetReviewableTicketAsync(userId, concertId);

        return result.IsSuccess;
    }

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        ticketModule.CanReviewArtistAsync(userId, artistId);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        ticketModule.CanReviewVenueAsync(userId, venueId);
}
