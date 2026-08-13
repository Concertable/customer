using Concertable.Contracts;
using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Identity;
using Reunion;

namespace Concertable.Customer.Review.Infrastructure.Services;

internal sealed class ConcertReviewService : IConcertReviewService
{
    private readonly IConcertReviewRepository reviewRepository;
    private readonly ITicketModule ticketModule;
    private readonly IReviewValidator reviewValidator;
    private readonly ICurrentUser currentUser;

    public ConcertReviewService(
        IConcertReviewRepository reviewRepository,
        ITicketModule ticketModule,
        IReviewValidator reviewValidator,
        ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.ticketModule = ticketModule;
        this.reviewValidator = reviewValidator;
        this.currentUser = currentUser;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams) =>
        reviewRepository.GetByConcertAsync(concertId, pageParams);

    public Task<ReviewSummary> GetSummaryAsync(int concertId) =>
        reviewRepository.GetSummaryByConcertAsync(concertId);

    public async Task<bool> CanCurrentUserReviewAsync(int concertId) =>
        currentUser.IsAuthenticated
        && (await GetReviewableTicketAsync(currentUser.GetId(), concertId)).IsSuccess;

    public Task<Result<ReviewDto, CreateReviewError>> CreateAsync(int concertId, CreateReviewRequest request)
    {
        var userId = currentUser.GetId();

        return GetReviewableTicketAsync(userId, concertId)
            .BindAsync(ticket => CreateAsync(ticket, request));
    }

    private async Task<Result<TicketSummary, CreateReviewError>> GetReviewableTicketAsync(
        Guid userId,
        int concertId)
    {
        var ticketOption = await ticketModule.GetByUserAndConcertAsync(userId, concertId);
        if (!ticketOption.TryGetValue(out var ticket))
            return new CreateReviewError.TicketNotFound();

        if (reviewValidator.ValidateReviewPeriod(ticket).IsInvalid)
            return new CreateReviewError.ConcertNotReviewableYet();

        if ((await reviewValidator.ValidateTicketNotReviewedAsync(ticket.Id)).IsInvalid)
            return new CreateReviewError.ReviewAlreadyExists();

        return ticket;
    }

    private async Task<Result<ReviewDto, CreateReviewError>> CreateAsync(
        TicketSummary ticket,
        CreateReviewRequest request)
    {
        var email = currentUser.Email
            ?? throw new UnauthorizedAccessException("User email claim missing.");

        var reviewResult = ReviewEntity.Create(
            ticket.Id,
            request.Stars,
            request.Details,
            email,
            artistId: ticket.ArtistId,
            venueId: ticket.VenueId,
            concertId: ticket.ConcertId)
            .MapError<CreateReviewError>(errors => new CreateReviewError.Invalid(errors));

        return await reviewResult.Match(
            success: PersistAsync,
            failure: error => Task.FromResult<Result<ReviewDto, CreateReviewError>>(error));
    }

    private async Task<Result<ReviewDto, CreateReviewError>> PersistAsync(ReviewEntity review)
    {
        if (!await reviewRepository.InsertAsync(review))
            return new CreateReviewError.ReviewAlreadyExists();

        return review.ToDto();
    }
}
