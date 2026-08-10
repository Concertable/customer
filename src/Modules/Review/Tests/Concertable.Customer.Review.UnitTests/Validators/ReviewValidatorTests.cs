using Concertable.Customer.Review.Application.Interfaces;
using Concertable.Customer.Review.Infrastructure.Validators;
using Concertable.Customer.Ticket.Contracts;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Reunion.Validation;

namespace Concertable.Customer.Review.UnitTests.Validators;

public sealed class ReviewValidatorTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const int ArtistId = 5;
    private const int VenueId = 7;

    private readonly FakeTimeProvider timeProvider;
    private readonly Mock<IConcertReviewRepository> concertReviewRepository;
    private readonly Mock<ITicketModule> ticketModule;
    private readonly ReviewValidator sut;

    public ReviewValidatorTests()
    {
        this.timeProvider = new FakeTimeProvider();
        this.concertReviewRepository = new Mock<IConcertReviewRepository>();
        this.ticketModule = new Mock<ITicketModule>();
        this.sut = new ReviewValidator(
            this.concertReviewRepository.Object,
            this.ticketModule.Object,
            this.timeProvider);
    }

    #region ValidateReviewPeriod

    [Fact]
    public void ValidateReviewPeriod_StartedConcert_ReturnsValid()
    {
        var ticket = NewTicket(this.timeProvider.GetUtcNow().UtcDateTime.AddDays(-1));

        var result = this.sut.ValidateReviewPeriod(ticket);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateReviewPeriod_FutureConcert_ReturnsStructuredInvalid()
    {
        var ticket = NewTicket(this.timeProvider.GetUtcNow().UtcDateTime.AddDays(1));

        var result = this.sut.ValidateReviewPeriod(ticket);

        AssertInvalid(result, "ConcertId", "The concert is not reviewable yet.");
    }

    #endregion

    #region ValidateTicketNotReviewedAsync

    [Fact]
    public async Task ValidateTicketNotReviewedAsync_UnreviewedTicket_ReturnsValid()
    {
        var ticketId = Guid.NewGuid();
        this.concertReviewRepository
            .Setup(repository => repository.HasReviewForTicketAsync(ticketId))
            .ReturnsAsync(false);

        var result = await this.sut.ValidateTicketNotReviewedAsync(ticketId);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateTicketNotReviewedAsync_ReviewedTicket_ReturnsStructuredInvalid()
    {
        var ticketId = Guid.NewGuid();
        this.concertReviewRepository
            .Setup(repository => repository.HasReviewForTicketAsync(ticketId))
            .ReturnsAsync(true);

        var result = await this.sut.ValidateTicketNotReviewedAsync(ticketId);

        AssertInvalid(result, "TicketId", "A review already exists for this ticket.");
    }

    [Fact]
    public async Task ValidateTicketNotReviewedAsync_RepositoryFault_Propagates()
    {
        var expected = new InvalidOperationException();
        this.concertReviewRepository
            .Setup(repository => repository.HasReviewForTicketAsync(It.IsAny<Guid>()))
            .ThrowsAsync(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.ValidateTicketNotReviewedAsync(Guid.NewGuid()));

        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task ValidateTicketNotReviewedAsync_CancelledQuery_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        this.concertReviewRepository
            .Setup(repository => repository.HasReviewForTicketAsync(It.IsAny<Guid>()))
            .Returns(Task.FromCanceled<bool>(cancellation.Token));

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.sut.ValidateTicketNotReviewedAsync(Guid.NewGuid()));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    #endregion

    #region ValidateArtistAsync

    [Fact]
    public async Task ValidateArtistAsync_ReviewableArtist_ReturnsValid()
    {
        this.ticketModule
            .Setup(module => module.CanReviewArtistAsync(UserId, ArtistId))
            .ReturnsAsync(true);

        var result = await this.sut.ValidateArtistAsync(UserId, ArtistId);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateArtistAsync_UnreviewableArtist_ReturnsStructuredInvalid()
    {
        this.ticketModule
            .Setup(module => module.CanReviewArtistAsync(UserId, ArtistId))
            .ReturnsAsync(false);

        var result = await this.sut.ValidateArtistAsync(UserId, ArtistId);

        AssertInvalid(result, "ArtistId", "No reviewable ticket exists for this artist.");
    }

    #endregion

    #region ValidateVenueAsync

    [Fact]
    public async Task ValidateVenueAsync_ReviewableVenue_ReturnsValid()
    {
        this.ticketModule
            .Setup(module => module.CanReviewVenueAsync(UserId, VenueId))
            .ReturnsAsync(true);

        var result = await this.sut.ValidateVenueAsync(UserId, VenueId);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateVenueAsync_UnreviewableVenue_ReturnsStructuredInvalid()
    {
        this.ticketModule
            .Setup(module => module.CanReviewVenueAsync(UserId, VenueId))
            .ReturnsAsync(false);

        var result = await this.sut.ValidateVenueAsync(UserId, VenueId);

        AssertInvalid(result, "VenueId", "No reviewable ticket exists for this venue.");
    }

    #endregion

    private static TicketSummary NewTicket(DateTime periodStart) =>
        new(Guid.NewGuid(), 1, ArtistId, VenueId, periodStart);

    private static void AssertInvalid(
        ValidationResult result,
        string field,
        string message)
    {
        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal([message], errors.Errors[field]);
    }
}
