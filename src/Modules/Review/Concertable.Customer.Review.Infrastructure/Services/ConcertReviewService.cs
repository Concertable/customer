using Concertable.Contracts;
using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Ticket.Contracts;
using Reunion;
using Concertable.Kernel.Identity;

namespace Concertable.Customer.Review.Infrastructure.Services;

internal sealed class ConcertReviewService : IConcertReviewService
{
    private readonly IConcertReviewRepository reviewRepository;
    private readonly IReviewValidator reviewValidator;
    private readonly ICurrentUser currentUser;

    public ConcertReviewService(
        IConcertReviewRepository reviewRepository,
        IReviewValidator reviewValidator,
        ICurrentUser currentUser)
    {
        this.reviewRepository = reviewRepository;
        this.reviewValidator = reviewValidator;
        this.currentUser = currentUser;
    }

    public Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams) =>
        reviewRepository.GetByConcertAsync(concertId, pageParams);

    public Task<ReviewSummary> GetSummaryAsync(int concertId) =>
        reviewRepository.GetSummaryByConcertAsync(concertId);

    public Task<bool> CanCurrentUserReviewAsync(int concertId) =>
        currentUser.IsAuthenticated
            ? reviewValidator.CanUserReviewConcertAsync(currentUser.GetId(), concertId)
            : Task.FromResult(false);

    public Task<Result<ReviewDto, CreateReviewError>> CreateAsync(int concertId, CreateReviewRequest request)
    {
        var userId = currentUser.GetId();

        return reviewValidator
            .GetReviewableTicketAsync(userId, concertId)
            .BindAsync(ticket => CreateAsync(ticket, request));
    }

    private async Task<Result<ReviewDto, CreateReviewError>> CreateAsync(
        TicketSummary ticket,
        CreateReviewRequest request)
    {
        var email = currentUser.Email
            ?? throw new UnauthorizedAccessException("User email claim missing.");

        var review = ReviewEntity.Create(
            ticket.Id,
            request.Stars,
            request.Details,
            email,
            artistId: ticket.ArtistId,
            venueId: ticket.VenueId,
            concertId: ticket.ConcertId);

        await reviewRepository.AddAsync(review);
        await reviewRepository.SaveChangesAsync();

        return Result.Success<ReviewDto, CreateReviewError>(review.ToDto());
    }
}
