using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Customer.Ticket.Application.Interfaces;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Infrastructure;
using Concertable.Customer.Ticket.Infrastructure.Services;
using Concertable.Kernel.Identity;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Client;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Errors;
using Concertable.Shared.QrCode.Application;
using Grpc.Core;
using Moq;
using Reunion;

namespace Concertable.Customer.Ticket.UnitTests;

public sealed class TicketServiceTests
{
    private readonly Mock<ITicketValidator> ticketValidator = new();
    private readonly Mock<IConcertModule> concertModule = new();
    private readonly Mock<ICustomerPaymentOperationsClient> customerPaymentClient = new();
    private readonly TicketService ticketService;

    public TicketServiceTests()
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.SetupGet(user => user.Id).Returns(Guid.NewGuid());
        currentUser.SetupGet(user => user.Email).Returns("customer@example.com");

        this.ticketValidator
            .Setup(validator => validator.CanPurchaseTickets(It.IsAny<ConcertDto>(), It.IsAny<int>()))
            .Returns(UnitResult<IReadOnlyList<string>>.Success());
        this.customerPaymentClient
            .Setup(client => client.PayAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<Money>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentOutcome, PaymentError>.Success(new PaymentOutcome()));
        this.customerPaymentClient
            .Setup(client => client.CreatePaymentSessionAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckoutSession("client-secret", "customer-session", "customer"));

        this.ticketService = new TicketService(
            new Mock<ITicketRepository>().Object,
            this.ticketValidator.Object,
            new Mock<IQrCodeGenerator>().Object,
            currentUser.Object,
            this.concertModule.Object,
            this.customerPaymentClient.Object,
            new Mock<IOutboxUnitOfWorkBehavior>().Object,
            new Mock<IBus>().Object,
            TimeProvider.System);
    }

    [Fact]
    public async Task PurchaseAsync_MissingConcert_ReturnsConcertNotFound()
    {
        var result = await this.ticketService.PurchaseAsync(new TicketPurchaseParams
        {
            ConcertId = 42,
            PaymentMethodId = "pm_test"
        });

        Assert.True(result.TryGetError(out var error));
        var notFound = Assert.IsType<PurchaseError.ConcertNotFound>(error);
        Assert.Equal(42, notFound.ConcertId);
    }

    [Fact]
    public async Task PurchaseAsync_InvalidPurchase_ReturnsValidationMessages()
    {
        var concert = CreateConcert();
        this.concertModule.Setup(module => module.GetByIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        this.ticketValidator.Setup(validator => validator.CanPurchaseTickets(concert, 2))
            .Returns(UnitResult<IReadOnlyList<string>>.Failure(["Not enough tickets available."]));

        var result = await this.ticketService.PurchaseAsync(new TicketPurchaseParams
        {
            ConcertId = concert.Id,
            Quantity = 2,
            PaymentMethodId = "pm_test"
        });

        Assert.True(result.TryGetError(out var error));
        var invalid = Assert.IsType<PurchaseError.Invalid>(error);
        Assert.Equal(["Not enough tickets available."], invalid.Errors.Errors["purchase"]);
    }

    [Fact]
    public async Task PurchaseAsync_PaymentRejected_ReturnsTicketPaymentRejected()
    {
        var concert = CreateConcert();
        this.concertModule.Setup(module => module.GetByIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        this.customerPaymentClient
            .Setup(client => client.PayAsync(
                It.IsAny<Guid>(),
                concert.Id,
                It.IsAny<Guid>(),
                It.IsAny<Money>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentOutcome, PaymentError>.Failure(new PaymentError.PaymentRejected()));

        var result = await this.ticketService.PurchaseAsync(new TicketPurchaseParams
        {
            ConcertId = concert.Id,
            PaymentMethodId = "pm_test"
        });

        Assert.True(result.TryGetError(out var error));
        Assert.IsType<PurchaseError.PaymentRejected>(error);
    }

    [Fact]
    public async Task PurchaseAsync_OtherPaymentFailure_PreservesPaymentError()
    {
        var concert = CreateConcert();
        var paymentError = new PaymentError.PayerNotFound();
        this.concertModule.Setup(module => module.GetByIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        this.customerPaymentClient
            .Setup(client => client.PayAsync(
                It.IsAny<Guid>(),
                concert.Id,
                It.IsAny<Guid>(),
                It.IsAny<Money>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaymentOutcome, PaymentError>.Failure(paymentError));

        var result = await this.ticketService.PurchaseAsync(new TicketPurchaseParams
        {
            ConcertId = concert.Id,
            PaymentMethodId = "pm_test"
        });

        Assert.True(result.TryGetError(out var error));
        var paymentFailure = Assert.IsType<PurchaseError.PaymentFailure>(error);
        Assert.Same(paymentError, paymentFailure.Error);
    }

    [Fact]
    public async Task PurchaseAsync_GrpcUnavailable_PropagatesException()
    {
        var concert = CreateConcert();
        var exception = new RpcException(new Status(StatusCode.Unavailable, "Unavailable"));
        this.concertModule.Setup(module => module.GetByIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        this.customerPaymentClient
            .Setup(client => client.PayAsync(
                It.IsAny<Guid>(),
                concert.Id,
                It.IsAny<Guid>(),
                It.IsAny<Money>(),
                It.IsAny<IReadOnlyDictionary<string, string>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = await Assert.ThrowsAsync<RpcException>(() =>
            this.ticketService.PurchaseAsync(new TicketPurchaseParams
            {
                ConcertId = concert.Id,
                PaymentMethodId = "pm_test"
            }));

        Assert.Same(exception, thrown);
    }

    [Fact]
    public async Task CheckoutAsync_MissingConcert_ReturnsConcertNotFound()
    {
        var result = await this.ticketService.CheckoutAsync(42, 1);

        Assert.True(result.TryGetError(out var error));
        var notFound = Assert.IsType<CheckoutError.ConcertNotFound>(error);
        Assert.Equal(42, notFound.ConcertId);
    }

    [Fact]
    public async Task CheckoutAsync_InvalidCheckout_ReturnsValidationMessages()
    {
        var concert = CreateConcert();
        this.concertModule.Setup(module => module.GetByIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        this.ticketValidator.Setup(validator => validator.CanPurchaseTickets(concert, 2))
            .Returns(UnitResult<IReadOnlyList<string>>.Failure(["Not enough tickets available."]));

        var result = await this.ticketService.CheckoutAsync(concert.Id, 2);

        Assert.True(result.TryGetError(out var error));
        var invalid = Assert.IsType<CheckoutError.Invalid>(error);
        Assert.Equal(["Not enough tickets available."], invalid.Errors.Errors["checkout"]);
    }

    [Fact]
    public async Task CompleteAsync_MissingConcert_ThrowsConsistencyFault()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.ticketService.CompleteAsync(new PurchaseComplete
            {
                EntityId = 42,
                FromEmail = "customer@example.com",
                FromUserId = Guid.NewGuid()
            }));

        Assert.Equal(
            "Concert 42 was not found while completing a ticket purchase.",
            exception.Message);
    }

    private static ConcertDto CreateConcert() => new(
        42,
        "Concert",
        25m,
        new DateRange(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)),
        DateTime.UtcNow,
        10,
        1,
        "Artist",
        2,
        "Venue",
        Guid.NewGuid(),
        Guid.NewGuid());
}
