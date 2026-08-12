using Concertable.Customer.Review.Application.Errors;
using Concertable.Customer.Review.Application.Interfaces;
using Concertable.Customer.Review.Application.Requests;
using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Review.Infrastructure.Services;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Identity;
using Moq;
using Reunion.Errors;
using Reunion.Validation;

namespace Concertable.Customer.Review.UnitTests.Services;

public sealed class ConcertReviewServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const int ConcertId = 1;

    private readonly TicketSummary ticket;
    private readonly Mock<IConcertReviewRepository> reviewRepository;
    private readonly Mock<ITicketModule> ticketModule;
    private readonly Mock<IReviewValidator> reviewValidator;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly ConcertReviewService sut;

    public ConcertReviewServiceTests()
    {
        this.ticket = NewTicket();
        this.reviewRepository = new Mock<IConcertReviewRepository>();
        this.ticketModule = new Mock<ITicketModule>();
        this.reviewValidator = new Mock<IReviewValidator>();
        this.currentUser = new Mock<ICurrentUser>();
        this.currentUser.SetupGet(user => user.Id).Returns(UserId);
        this.currentUser.SetupGet(user => user.IsAuthenticated).Returns(true);
        this.currentUser.SetupGet(user => user.Email).Returns("customer@example.com");
        this.ticketModule
            .Setup(module => module.GetByUserAndConcertAsync(UserId, ConcertId))
            .ReturnsAsync(this.ticket);
        this.reviewValidator
            .Setup(validator => validator.ValidateReviewPeriod(this.ticket))
            .Returns(ValidationResult.Valid());
        this.reviewValidator
            .Setup(validator => validator.ValidateTicketNotReviewedAsync(this.ticket.Id))
            .ReturnsAsync(ValidationResult.Valid());
        this.reviewRepository
            .Setup(repository => repository.InsertAsync(It.IsAny<ReviewEntity>()))
            .ReturnsAsync(true);
        this.sut = new ConcertReviewService(
            this.reviewRepository.Object,
            this.ticketModule.Object,
            this.reviewValidator.Object,
            this.currentUser.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_MissingTicket_ReturnsTicketNotFound()
    {
        this.ticketModule
            .Setup(module => module.GetByUserAndConcertAsync(UserId, ConcertId))
            .ReturnsAsync((TicketSummary?)null);

        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<CreateReviewError.TicketNotFound>(error);
        this.reviewValidator.Verify(
            validator => validator.ValidateReviewPeriod(It.IsAny<TicketSummary>()),
            Times.Never);
        this.reviewRepository.Verify(
            repository => repository.InsertAsync(It.IsAny<ReviewEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidReviewPeriod_ReturnsConcertNotReviewableYet()
    {
        this.reviewValidator
            .Setup(validator => validator.ValidateReviewPeriod(this.ticket))
            .Returns(Invalid("ConcertId", "The concert is not reviewable yet."));

        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<CreateReviewError.ConcertNotReviewableYet>(error);
        this.reviewValidator.Verify(
            validator => validator.ValidateTicketNotReviewedAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ReviewedTicket_ReturnsReviewAlreadyExists()
    {
        this.reviewValidator
            .Setup(validator => validator.ValidateTicketNotReviewedAsync(this.ticket.Id))
            .ReturnsAsync(Invalid("TicketId", "A review already exists for this ticket."));

        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<CreateReviewError.ReviewAlreadyExists>(error);
        this.reviewRepository.Verify(
            repository => repository.InsertAsync(It.IsAny<ReviewEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_StarsOutOfRange_ReturnsInvalid()
    {
        var result = await this.sut.CreateAsync(ConcertId, NewRequest(stars: 6));

        Assert.True(result.TryGetError(out var error));
        var invalid = Assert.IsType<CreateReviewError.Invalid>(error);
        Assert.Equal(
            ["Stars must be between 1 and 5."],
            invalid.Errors.Errors["Stars"]);
        this.reviewRepository.Verify(
            repository => repository.InsertAsync(It.IsAny<ReviewEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ReviewableTicket_PersistsAndReturnsReview()
    {
        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetValue(out var review));
        Assert.Equal("customer@example.com", review.Email);
        Assert.Equal(4, review.Stars);
        Assert.Equal("Great concert", review.Details);
        this.ticketModule.Verify(
            module => module.GetByUserAndConcertAsync(UserId, ConcertId),
            Times.Once);
        this.reviewRepository.Verify(
            repository => repository.InsertAsync(It.Is<ReviewEntity>(entity =>
                entity.TicketId == this.ticket.Id
                && entity.ConcertId == this.ticket.ConcertId
                && entity.ArtistId == this.ticket.ArtistId
                && entity.VenueId == this.ticket.VenueId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateInsert_ReturnsReviewAlreadyExists()
    {
        this.reviewRepository
            .Setup(repository => repository.InsertAsync(It.IsAny<ReviewEntity>()))
            .ReturnsAsync(false);

        var result = await this.sut.CreateAsync(ConcertId, NewRequest());

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<CreateReviewError.ReviewAlreadyExists>(error);
    }

    [Fact]
    public async Task CreateAsync_PersistenceFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.reviewRepository
            .Setup(repository => repository.InsertAsync(It.IsAny<ReviewEntity>()))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task CreateAsync_CancelledPersistence_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        this.reviewRepository
            .Setup(repository => repository.InsertAsync(It.IsAny<ReviewEntity>()))
            .Returns(Task.FromCanceled<bool>(cancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task CreateAsync_MissingEmailClaim_ThrowsUnauthorizedAccessException()
    {
        this.currentUser.SetupGet(user => user.Email).Returns((string?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));
    }

    [Fact]
    public async Task CreateAsync_TicketModuleFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.ticketModule
            .Setup(module => module.GetByUserAndConcertAsync(UserId, ConcertId))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task CreateAsync_ReviewPeriodValidatorFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.reviewValidator
            .Setup(validator => validator.ValidateReviewPeriod(this.ticket))
            .Throws(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task CreateAsync_CancelledReviewQuery_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        this.reviewValidator
            .Setup(validator => validator.ValidateTicketNotReviewedAsync(this.ticket.Id))
            .Returns(Task.FromCanceled<ValidationResult>(cancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.sut.CreateAsync(ConcertId, NewRequest()));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    #endregion

    #region CanCurrentUserReviewAsync

    [Fact]
    public async Task CanCurrentUserReviewAsync_UnauthenticatedUser_ReturnsFalseWithoutTicketQuery()
    {
        this.currentUser.SetupGet(user => user.IsAuthenticated).Returns(false);

        var result = await this.sut.CanCurrentUserReviewAsync(ConcertId);

        Assert.False(result);
        this.ticketModule.Verify(
            module => module.GetByUserAndConcertAsync(It.IsAny<Guid>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_ReviewableTicket_ReturnsTrueWithOneTicketQuery()
    {
        var result = await this.sut.CanCurrentUserReviewAsync(ConcertId);

        Assert.True(result);
        this.ticketModule.Verify(
            module => module.GetByUserAndConcertAsync(UserId, ConcertId),
            Times.Once);
    }

    [Fact]
    public async Task CanCurrentUserReviewAsync_InvalidReviewPeriod_ReturnsFalse()
    {
        this.reviewValidator
            .Setup(validator => validator.ValidateReviewPeriod(this.ticket))
            .Returns(Invalid("ConcertId", "The concert is not reviewable yet."));

        var result = await this.sut.CanCurrentUserReviewAsync(ConcertId);

        Assert.False(result);
        this.reviewValidator.Verify(
            validator => validator.ValidateTicketNotReviewedAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    #endregion

    private static TicketSummary NewTicket() =>
        new(Guid.NewGuid(), ConcertId, 5, 7, DateTime.UtcNow.AddDays(-1));

    private static CreateReviewRequest NewRequest(byte stars = 4) => new()
    {
        Stars = stars,
        Details = "Great concert"
    };

    private static ValidationResult Invalid(string field, string message) =>
        ValidationResult.Invalid(new ValidationErrors([new(field, message)]));
}
