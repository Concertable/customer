using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Identity;
using Concertable.Kernel.Exceptions;

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

    public Task<bool> CanCurrentUserReviewAsync(int concertId) =>
        reviewValidator.CanUserReviewConcertAsync(currentUser.GetId(), concertId);

    public async Task<ReviewDto> CreateAsync(int concertId, CreateReviewRequest request)
    {
        var userId = currentUser.GetId();
        var ticket = await ticketModule.GetByUserAndConcertAsync(userId, concertId)
            ?? throw new NotFoundException("Cannot find ticket");

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

        return review.ToDto();
    }
}
