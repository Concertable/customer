using Concertable.Contracts;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Ticket.Application.Interfaces;
using Concertable.Kernel.Identity;
using Concertable.Kernel.Exceptions;

namespace Concertable.Customer.Review.Infrastructure.Services;

internal sealed class ConcertReviewService(
    IConcertReviewRepository reviewRepository,
    ITicketRepository ticketRepository,
    IReviewValidator reviewValidator,
    ICurrentUser currentUser) : IConcertReviewService
{
    public Task<IPagination<ReviewDto>> GetAsync(int concertId, IPageParams pageParams) =>
        reviewRepository.GetByConcertAsync(concertId, pageParams);

    public Task<ReviewSummaryDto> GetSummaryAsync(int concertId) =>
        reviewRepository.GetSummaryByConcertAsync(concertId);

    public Task<bool> CanCurrentUserReviewAsync(int concertId) =>
        reviewValidator.CanUserReviewConcertAsync(currentUser.GetId(), concertId);

    public async Task<ReviewDto> CreateAsync(CreateReviewRequest request)
    {
        var userId = currentUser.GetId();
        var ticket = await ticketRepository.GetByUserIdAndConcertIdAsync(userId, request.ConcertId)
            ?? throw new NotFoundException("Cannot find ticket");

        var review = ReviewEntity.Create(
            ticket.Id,
            request.Stars,
            request.Details,
            currentUser.Email ?? string.Empty,
            artistId: ticket.ArtistId,
            venueId: ticket.VenueId,
            concertId: ticket.ConcertId);

        await reviewRepository.AddAsync(review);
        await reviewRepository.SaveChangesAsync();

        return review.ToDto();
    }
}
