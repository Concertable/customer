using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Infrastructure.Validators;
using Concertable.Kernel;
using Concertable.Kernel.Exceptions;
using Concertable.Testing;
using Moq;

namespace Concertable.Customer.Ticket.UnitTests.Validators;

public sealed class TicketValidatorTests
{
    private static readonly DateTimeOffset Now = new FakeTimeProvider().GetUtcNow();
    private static readonly Guid PayeeUserId = Guid.NewGuid();

    private readonly Mock<IConcertModule> concertModule;
    private readonly TicketValidator sut;

    public TicketValidatorTests()
    {
        concertModule = new Mock<IConcertModule>();
        sut = new TicketValidator(concertModule.Object, new FakeTimeProvider());
    }

    private static ConcertDto NewConcert(
        bool posted = true,
        int availableTickets = 10,
        DateRange? period = null) =>
        new(
            1,
            "Concert",
            25m,
            period ?? new DateRange(Now.UtcDateTime.AddDays(7), Now.UtcDateTime.AddDays(8)),
            posted ? Now.UtcDateTime.AddDays(-30) : null,
            availableTickets,
            5,
            "Artist",
            7,
            "Venue",
            PayeeUserId);

    [Fact]
    public void CanBePurchased_WithPostedUpcomingConcertInStock_Succeeds()
    {
        var result = sut.CanBePurchased(NewConcert());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanBePurchased_WhenNotPosted_Fails()
    {
        var result = sut.CanBePurchased(NewConcert(posted: false));

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void CanBePurchased_WhenConcertAlreadyStarted_Fails()
    {
        var started = new DateRange(Now.UtcDateTime.AddDays(-1), Now.UtcDateTime.AddDays(1));

        var result = sut.CanBePurchased(NewConcert(period: started));

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void CanBePurchased_WhenNoAvailability_Fails()
    {
        var result = sut.CanBePurchased(NewConcert(availableTickets: 0));

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void CanBePurchased_AccumulatesAllFailures()
    {
        var started = new DateRange(Now.UtcDateTime.AddDays(-1), Now.UtcDateTime.AddDays(1));

        var result = sut.CanBePurchased(NewConcert(posted: false, availableTickets: 0, period: started));

        Assert.True(result.IsFailed);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void CanPurchaseTickets_WithExactlyEnoughStock_Succeeds()
    {
        var result = sut.CanPurchaseTickets(NewConcert(availableTickets: 10), quantity: 10);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanPurchaseTickets_WhenQuantityExceedsAvailability_Fails()
    {
        var result = sut.CanPurchaseTickets(NewConcert(availableTickets: 10), quantity: 11);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void CanPurchaseTickets_WhenBaseValidationFails_ReturnsBaseFailure()
    {
        var result = sut.CanPurchaseTickets(NewConcert(posted: false), quantity: 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task CanBePurchasedAsync_WhenConcertMissing_ThrowsNotFound()
    {
        // Arrange
        concertModule.Setup(m => m.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertDto?)null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.CanBePurchasedAsync(999));
    }

    [Fact]
    public async Task CanBePurchasedAsync_ValidatesFetchedConcert()
    {
        // Arrange
        concertModule.Setup(m => m.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewConcert());

        // Act
        var result = await sut.CanBePurchasedAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
