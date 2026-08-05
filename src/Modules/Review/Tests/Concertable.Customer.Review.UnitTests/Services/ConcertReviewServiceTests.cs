using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Review.Application.Interfaces;
using Concertable.Customer.Review.Application.Requests;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Review.Infrastructure.Services;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Functional;
using Concertable.Kernel.Identity;
using Moq;

namespace Concertable.Customer.Review.UnitTests.Services;

public sealed class ConcertReviewServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const int ConcertId = 1;

    private readonly Mock<IConcertReviewRepository> reviewRepository;
    private readonly Mock<IReviewValidator> reviewValidator;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly ConcertReviewService sut;

    public ConcertReviewServiceTests()
    {
        this.reviewRepository = new Mock<IConcertReviewRepository>();
        this.reviewValidator = new Mock<IReviewValidator>();
        this.currentUser = new Mock<ICurrentUser>();
        this.currentUser.SetupGet(user => user.Id).Returns(UserId);
        this.currentUser.SetupGet(user => user.Email).Returns("customer@example.com");
        this.reviewRepository
            .Setup(repository => repository.AddAsync(It.IsAny<ReviewEntity>()))
            .ReturnsAsync((ReviewEntity review) => review);
        this.reviewRepository
            .Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        this.sut = new ConcertReviewService(
            reviewRepository.Object,
            reviewValidator.Object,
            currentUser.Object);
    }

    [Fact]
    public async Task CreateAsync_UnreviewableTicket_ReturnsValidatorFailure()
    {
        this.reviewValidator
            .Setup(validator => validator.GetReviewableTicketAsync(UserId, ConcertId))
            .ReturnsAsync(Result.Failure<TicketSummary, CreateReviewError>(
                CreateReviewError.ConcertNotReviewableYet));

        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.Same(CreateReviewError.ConcertNotReviewableYet, error);
        this.reviewRepository.Verify(
            repository => repository.AddAsync(It.IsAny<ReviewEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ReviewableTicket_PersistsAndReturnsReview()
    {
        var ticket = NewTicket();
        this.reviewValidator
            .Setup(validator => validator.GetReviewableTicketAsync(UserId, ConcertId))
            .ReturnsAsync(Result.Success<TicketSummary, CreateReviewError>(ticket));

        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetValue(out var review));
        Assert.Equal("customer@example.com", review.Email);
        Assert.Equal(4, review.Stars);
        Assert.Equal("Great concert", review.Details);
        this.reviewRepository.Verify(
            repository => repository.AddAsync(It.Is<ReviewEntity>(entity =>
                entity.TicketId == ticket.Id
                && entity.ConcertId == ticket.ConcertId
                && entity.ArtistId == ticket.ArtistId
                && entity.VenueId == ticket.VenueId)),
            Times.Once);
        this.reviewRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MissingEmailClaim_ThrowsUnauthorizedAccessException()
    {
        this.currentUser.SetupGet(user => user.Email).Returns((string?)null);
        this.reviewValidator
            .Setup(validator => validator.GetReviewableTicketAsync(UserId, ConcertId))
            .ReturnsAsync(Result.Success<TicketSummary, CreateReviewError>(NewTicket()));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));
    }

    [Fact]
    public async Task CreateAsync_ValidatorFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.reviewValidator
            .Setup(validator => validator.GetReviewableTicketAsync(UserId, ConcertId))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task CreateAsync_CancelledSave_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        this.reviewValidator
            .Setup(validator => validator.GetReviewableTicketAsync(UserId, ConcertId))
            .ReturnsAsync(Result.Success<TicketSummary, CreateReviewError>(NewTicket()));
        this.reviewRepository
            .Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.FromCanceled(cancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    private static TicketSummary NewTicket() =>
        new(Guid.NewGuid(), ConcertId, 5, 7, DateTime.UtcNow.AddDays(-1));

    private static CreateReviewRequest NewRequest() => new()
    {
        Stars = 4,
        Details = "Great concert"
    };
}
