using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Customer.Ticket.Application.Payments;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Kernel.Identity;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Client;
using Concertable.Payment.Contracts;
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
    private readonly IPaymentSessionOperationsClient paymentSessions;
    private readonly IOutboxUnitOfWorkBehavior outboxBehavior;
    private readonly IBus bus;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IQrCodeGenerator qrCodeGenerator,
        ICurrentUser currentUser,
        IConcertModule concertModule,
        IPaymentSessionOperationsClient paymentSessions,
        IOutboxUnitOfWorkBehavior outboxBehavior,
        IBus bus,
        TimeProvider timeProvider)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.qrCodeGenerator = qrCodeGenerator;
        this.currentUser = currentUser;
        this.concertModule = concertModule;
        this.paymentSessions = paymentSessions;
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
        var paymentResult = await CreatePaymentSessionAsync(concert, purchaseParams.Quantity);

        return paymentResult
            .Map(payment => new TicketPayment
            {
                Reference = payment.Reference,
                ClientSecret = payment.Session.ClientSecret,
                ConcertId = concert.Id,
                Amount = concert.Price * purchaseParams.Quantity,
                Currency = "GBP",
                UserEmail = currentUser.Email
            })
            .MapError<PurchaseError>(static error => error);
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
            if (purchaseCompleteDto.FromEmail is not null)
                await bus.SendAsync(new SendTicketEmailCommand(purchaseCompleteDto.FromEmail, ids));

            return ids;
        });

        return new TicketPayment
        {
            Reference = purchaseCompleteDto.Reference,
            TicketIds = ticketIds,
            ConcertId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = concert.Price * quantity,
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
            .BindAsync(concert => CreateCheckoutAsync(concert, quantity));

    private async Task<Result<TicketCheckout, CheckoutError>> CreateCheckoutAsync(ConcertDto concert, int quantity)
    {
        var paymentResult = await CreatePaymentSessionAsync(concert, quantity);

        return paymentResult
            .Map(payment => new TicketCheckout(
                payment.Reference,
                new CheckoutSession(
                    payment.Session.ClientSecret,
                    payment.Session.CustomerSessionSecret,
                    payment.Session.CustomerToken),
                concert.Price,
                concert.Id,
                quantity))
            .MapError<CheckoutError>(static error => error);
    }

    private async Task<Result<TicketPaymentSession, PaymentOperationError>> CreatePaymentSessionAsync(
        ConcertDto concert,
        int quantity)
    {
        var buyerId = currentUser.GetId();
        var reference = TicketPaymentOperationReferences.Create(
            TicketPaymentOperationType.Purchase,
            buyerId,
            concert.Id,
            quantity);
        var request = new PaymentSessionOperationRequest(
            Guid.CreateVersion7(timeProvider.GetUtcNow()),
            PaymentSessionKind.Payment,
            PaymentSession.OnSession,
            reference,
            buyerId,
            concert.PayeeOwnerId,
            Money.Gbp(concert.Price * quantity).ToMinorUnits(),
            Currency.Gbp,
            PaymentSessionFundsRouting.Destination);
        var result = await paymentSessions.CreateAsync(request);

        return result.Map(session => new TicketPaymentSession(reference, session));
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

    private static ValidationErrors CreateValidationErrors(string field, ValidationErrors errors) =>
        new(new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [field] = errors.Errors.SelectMany(error => error.Value).ToArray()
        });

    private sealed record TicketPaymentSession(
        PaymentOperationReference Reference,
        PaymentSessionDescriptor Session);
}
