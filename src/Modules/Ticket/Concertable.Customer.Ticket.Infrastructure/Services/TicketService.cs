using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Kernel.Identity;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Contracts.Errors;
using Concertable.Shared.QrCode.Application;
using Reunion;
using Reunion.Errors;
using Reunion.Validation;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IQrCodeGenerator qrCodeGenerator;
    private readonly ICurrentUser currentUser;
    private readonly IConcertModule concertModule;
    private readonly ICustomerPaymentOperationsClient customerPaymentClient;
    private readonly IOutboxUnitOfWorkBehavior outboxBehavior;
    private readonly IBus bus;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IQrCodeGenerator qrCodeGenerator,
        ICurrentUser currentUser,
        IConcertModule concertModule,
        ICustomerPaymentOperationsClient customerPaymentClient,
        IOutboxUnitOfWorkBehavior outboxBehavior,
        IBus bus,
        TimeProvider timeProvider)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.qrCodeGenerator = qrCodeGenerator;
        this.currentUser = currentUser;
        this.concertModule = concertModule;
        this.customerPaymentClient = customerPaymentClient;
        this.outboxBehavior = outboxBehavior;
        this.bus = bus;
        this.timeProvider = timeProvider;
    }

    public Task<Result<TicketPayment, PurchaseError>> PurchaseAsync(TicketPurchaseParams purchaseParams) =>
        concertModule.GetByIdAsync(purchaseParams.ConcertId)
            .OrFailure<ConcertDto, PurchaseError>(
                new PurchaseError.ConcertNotFound(purchaseParams.ConcertId))
            .Ensure(
                concert => ticketValidator.CanPurchaseTickets(concert, purchaseParams.Quantity),
                errors => new PurchaseError.Invalid(CreateValidationErrors("purchase", errors)))
            .BindAsync(concert => PayForTicketsAsync(concert, purchaseParams));

    private async Task<Result<TicketPayment, PurchaseError>> PayForTicketsAsync(
        ConcertDto concert,
        TicketPurchaseParams purchaseParams)
    {
        var metadata = new Dictionary<string, string>
        {
            [PaymentMetadataKeys.Type] = TransactionTypes.Ticket,
            [PaymentMetadataKeys.ConcertId] = concert.Id.ToString(),
            [PaymentMetadataKeys.Quantity] = purchaseParams.Quantity.ToString()
        };

        var paymentResult = await customerPaymentClient.PayAsync(
            currentUser.GetId(), concert.Id, concert.PayeeOwnerId,
            Money.Gbp(concert.Price * purchaseParams.Quantity),
            metadata,
            purchaseParams.PaymentMethodId);

        return paymentResult
            .Map(payment => new TicketPayment
            {
                RequiresAction = payment.RequiresAction,
                TransactionId = payment.TransactionId,
                ClientSecret = payment.ClientSecret,
                UserEmail = currentUser.Email
            })
            .MapError(ToPurchaseError);
    }

    public async Task<TicketPayment> CompleteAsync(PurchaseComplete purchaseCompleteDto)
    {
        var concertOption = await concertModule.GetByIdAsync(purchaseCompleteDto.EntityId);
        if (!concertOption.TryGetValue(out var concert))
            throw new InvalidOperationException(
                $"Concert {purchaseCompleteDto.EntityId} was not found while completing a ticket purchase.");

        int quantity = purchaseCompleteDto.Quantity ?? 1;
        var tickets = new List<TicketEntity>();

        var ticketIds = await outboxBehavior.ExecuteAsync(async () =>
        {
            for (int i = 0; i < quantity; i++)
            {
                var ticket = BuildTicket(purchaseCompleteDto.FromUserId, concert);
                await ticketRepository.AddAsync(ticket);
                tickets.Add(ticket);
            }

            var ids = tickets.Select(t => t.Id).ToList();
            await bus.SendAsync(new SendTicketEmailCommand(purchaseCompleteDto.FromEmail, ids));
            return ids;
        });

        return new TicketPayment
        {
            TicketIds = ticketIds,
            ConcertId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = concert.Price,
            Currency = "GBP",
            UserEmail = purchaseCompleteDto.FromEmail
        };
    }

    public Task<Result<TicketCheckout, CheckoutError>> CheckoutAsync(int concertId, int quantity) =>
        concertModule.GetByIdAsync(concertId)
            .OrFailure<ConcertDto, CheckoutError>(new CheckoutError.ConcertNotFound(concertId))
            .Ensure(
                concert => ticketValidator.CanPurchaseTickets(concert, quantity),
                errors => new CheckoutError.Invalid(CreateValidationErrors("checkout", errors)))
            .MapAsync(concert => CreateCheckoutAsync(concert, quantity));

    private async Task<TicketCheckout> CreateCheckoutAsync(ConcertDto concert, int quantity)
    {
        var metadata = new Dictionary<string, string>
        {
            [PaymentMetadataKeys.Type] = TransactionTypes.Ticket,
            [PaymentMetadataKeys.ConcertId] = concert.Id.ToString(),
            [PaymentMetadataKeys.Quantity] = quantity.ToString(),
            [PaymentMetadataKeys.Amount] = Money.Gbp(concert.Price * quantity).ToMinorUnits().ToString(),
            [PaymentMetadataKeys.Currency] = "gbp"
        };

        var session = await customerPaymentClient.CreatePaymentSessionAsync(currentUser.GetId(), concert.Id, concert.PayeeOwnerId, metadata);

        return new TicketCheckout(session, concert.Price, concert.Id, quantity);
    }

    public async Task<IEnumerable<TicketDto>> GetUserUpcomingAsync()
    {
        var tickets = await ticketRepository.GetUpcomingByUserIdAsync(currentUser.GetId());
        return tickets.ToDtos();
    }

    public async Task<IEnumerable<TicketDto>> GetUserHistoryAsync()
    {
        var tickets = await ticketRepository.GetHistoryByUserIdAsync(currentUser.GetId());
        return tickets.ToDtos();
    }

    public Task<Option<TicketSummary>> GetByUserAndConcertAsync(Guid userId, int concertId) =>
        ticketRepository.GetSummaryByUserAndConcertAsync(userId, concertId).ToOption();

    public Task<bool> CanReviewArtistAsync(Guid userId, int artistId) =>
        ticketRepository.CanReviewArtistAsync(userId, artistId);

    public Task<bool> CanReviewVenueAsync(Guid userId, int venueId) =>
        ticketRepository.CanReviewVenueAsync(userId, venueId);

    private TicketEntity BuildTicket(Guid userId, ConcertDto concert)
    {
        var ticketId = Guid.CreateVersion7();
        var qrCode = qrCodeGenerator.Generate(ticketId.ToString());
        return TicketEntity.Purchase(
            ticketId,
            userId,
            concert.Id,
            qrCode,
            timeProvider.GetUtcNow().DateTime,
            concert.Name,
            concert.Price,
            concert.Period,
            concert.ArtistId,
            concert.ArtistName,
            concert.VenueId,
            concert.VenueName);
    }

    private static PurchaseError ToPurchaseError(PaymentError error) =>
        error is PaymentError.PaymentRejected
            ? new PurchaseError.PaymentRejected()
            : new PurchaseError.PaymentFailure(error);

    private static ValidationErrors CreateValidationErrors(string field, ValidationErrors errors) =>
        new(new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [field] = errors.Errors.SelectMany(error => error.Value).ToArray()
        });
}
