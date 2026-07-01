using Concertable.Customer.Review.Application.Interfaces;
using Concertable.Customer.Review.Infrastructure.Validators;
using Concertable.Customer.Ticket.Contracts;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Concertable.Customer.Review.UnitTests.Validators;

public sealed class ReviewValidatorTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const int ConcertId = 1;

    private readonly FakeTimeProvider timeProvider;
    private readonly Mock<IConcertReviewRepository> concertReviewRepository;
    private readonly Mock<ITicketModule> ticketModule;
    private readonly ReviewValidator sut;

    public ReviewValidatorTests()
    {
        this.timeProvider = new FakeTimeProvider();
        this.concertReviewRepository = new Mock<IConcertReviewRepository>();
        this.ticketModule = new Mock<ITicketModule>();
        this.sut = new ReviewValidator(concertReviewRepository.Object, ticketModule.Object, timeProvider);
    }

    private static TicketSummary NewTicket(DateTime periodStart) =>
        new(Guid.NewGuid(), ConcertId, 5, 7, periodStart);

    [Fact]
    public async Task CanUserReviewConcertAsync_WithStartedConcertAndNoExistingReview_ReturnsTrue()
    {
        // Arrange
        var ticket = NewTicket(periodStart: timeProvider.GetUtcNow().UtcDateTime.AddDays(-1));
        ticketModule.Setup(m => m.GetByUserAndConcertAsync(UserId, ConcertId)).ReturnsAsync(ticket);
        concertReviewRepository.Setup(r => r.HasReviewForTicketAsync(ticket.Id)).ReturnsAsync(false);

        // Act
        var result = await sut.CanUserReviewConcertAsync(UserId, ConcertId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserReviewConcertAsync_WhenUserHasNoTicket_ReturnsFalse()
    {
        // Arrange
        ticketModule.Setup(m => m.GetByUserAndConcertAsync(UserId, ConcertId)).ReturnsAsync((TicketSummary?)null);

        // Act
        var result = await sut.CanUserReviewConcertAsync(UserId, ConcertId);

        // Assert
        Assert.False(result);
        concertReviewRepository.Verify(r => r.HasReviewForTicketAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CanUserReviewConcertAsync_WhenConcertNotStarted_ReturnsFalse()
    {
        // Arrange
        var ticket = NewTicket(periodStart: timeProvider.GetUtcNow().UtcDateTime.AddDays(1));
        ticketModule.Setup(m => m.GetByUserAndConcertAsync(UserId, ConcertId)).ReturnsAsync(ticket);

        // Act
        var result = await sut.CanUserReviewConcertAsync(UserId, ConcertId);

        // Assert
        Assert.False(result);
        concertReviewRepository.Verify(r => r.HasReviewForTicketAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CanUserReviewConcertAsync_WhenTicketAlreadyReviewed_ReturnsFalse()
    {
        // Arrange
        var ticket = NewTicket(periodStart: timeProvider.GetUtcNow().UtcDateTime.AddDays(-1));
        ticketModule.Setup(m => m.GetByUserAndConcertAsync(UserId, ConcertId)).ReturnsAsync(ticket);
        concertReviewRepository.Setup(r => r.HasReviewForTicketAsync(ticket.Id)).ReturnsAsync(true);

        // Act
        var result = await sut.CanUserReviewConcertAsync(UserId, ConcertId);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CanUserReviewArtistAsync_DelegatesToTicketModule(bool canReview)
    {
        // Arrange
        ticketModule.Setup(m => m.CanReviewArtistAsync(UserId, 5)).ReturnsAsync(canReview);

        // Act
        var result = await sut.CanUserReviewArtistAsync(UserId, 5);

        // Assert
        Assert.Equal(canReview, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CanUserReviewVenueAsync_DelegatesToTicketModule(bool canReview)
    {
        // Arrange
        ticketModule.Setup(m => m.CanReviewVenueAsync(UserId, 7)).ReturnsAsync(canReview);

        // Act
        var result = await sut.CanUserReviewVenueAsync(UserId, 7);

        // Assert
        Assert.Equal(canReview, result);
    }
}
