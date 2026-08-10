using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Customer.Ticket.Infrastructure.Validators;
using Concertable.Kernel.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Reunion.Errors;
using Reunion.Validation;

namespace Concertable.Customer.Ticket.UnitTests.Validators;

public sealed class TicketValidatorTests
{
    private static readonly Guid PayeeUserId = Guid.NewGuid();
    private static readonly Guid PayeeOwnerId = Guid.NewGuid();

    private readonly FakeTimeProvider timeProvider;
    private readonly Mock<IConcertModule> concertModule;
    private readonly TicketValidator sut;

    public TicketValidatorTests()
    {
        this.timeProvider = new FakeTimeProvider();
        this.concertModule = new Mock<IConcertModule>();
        this.sut = new TicketValidator(concertModule.Object, timeProvider);
    }

    private ConcertDto NewConcert(
        bool posted = true,
        int availableTickets = 10,
        DateRange? period = null) =>
        new(
            1,
            "Concert",
            25m,
            period ?? new DateRange(timeProvider.GetUtcNow().UtcDateTime.AddDays(7), timeProvider.GetUtcNow().UtcDateTime.AddDays(8)),
            posted ? timeProvider.GetUtcNow().UtcDateTime.AddDays(-30) : null,
            availableTickets,
            5,
            "Artist",
            7,
            "Venue",
            PayeeUserId,
            PayeeOwnerId);

    [Fact]
    public void CanBePurchased_WithPostedUpcomingConcertInStock_Succeeds()
    {
        var result = sut.CanBePurchased(NewConcert());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CanBePurchased_WhenNotPosted_Fails()
    {
        var result = sut.CanBePurchased(NewConcert(posted: false));

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void CanBePurchased_WhenConcertAlreadyStarted_Fails()
    {
        var started = new DateRange(timeProvider.GetUtcNow().UtcDateTime.AddDays(-1), timeProvider.GetUtcNow().UtcDateTime.AddDays(1));

        var result = sut.CanBePurchased(NewConcert(period: started));

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void CanBePurchased_WhenNoAvailability_Fails()
    {
        var result = sut.CanBePurchased(NewConcert(availableTickets: 0));

        Assert.True(result.IsInvalid);
    }

    [Fact]
    public void CanPurchaseTickets_WithSeveralBaseFailures_AccumulatesAllFailures()
    {
        var started = new DateRange(timeProvider.GetUtcNow().UtcDateTime.AddDays(-1), timeProvider.GetUtcNow().UtcDateTime.AddDays(1));

        var result = sut.CanPurchaseTickets(
            NewConcert(posted: false, availableTickets: 0, period: started),
            quantity: 1);

        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(3, errors.Errors["concert"].Count);
    }

    [Fact]
    public void CanPurchaseTickets_WithExactlyEnoughStock_Succeeds()
    {
        var result = sut.CanPurchaseTickets(NewConcert(availableTickets: 10), quantity: 10);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CanPurchaseTickets_WhenQuantityExceedsAvailability_Fails()
    {
        var result = sut.CanPurchaseTickets(NewConcert(availableTickets: 10), quantity: 11);

        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(
            ["Not enough tickets available. Only 10 tickets are available"],
            errors.Errors["quantity"]);
    }

    [Fact]
    public void CanPurchaseTickets_WhenBaseValidationFails_ReturnsBaseFailure()
    {
        var result = sut.CanPurchaseTickets(NewConcert(posted: false), quantity: 1);

        Assert.True(result.TryGetErrors(out var errors));
        Assert.Equal(["Concert is not posted yet"], errors.Errors["concert"]);
    }

    [Fact]
    public async Task CanBePurchasedAsync_WhenConcertMissing_ReturnsConcertNotFound()
    {
        concertModule.Setup(m => m.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertDto?)null);

        var result = await sut.CanBePurchasedAsync(999);

        Assert.True(result.TryGetError(out var error));
        var notFound = Assert.IsType<EligibilityError.ConcertNotFound>(error);
        Assert.Equal(999, notFound.ConcertId);
    }

    [Fact]
    public async Task CanBePurchasedAsync_ValidatesFetchedConcert()
    {
        concertModule.Setup(m => m.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewConcert());

        var result = await sut.CanBePurchasedAsync(1);

        Assert.True(result.TryGetValue(out var validation));
        Assert.True(validation.IsValid);
    }
}
