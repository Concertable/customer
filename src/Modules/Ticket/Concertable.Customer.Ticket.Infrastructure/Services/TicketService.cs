using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Kernel.Identity;
using Concertable.Kernel.ValueObjects;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Contracts.Errors;
using Concertable.Shared.QrCode.Application;
using Reunion;
using Reunion.Errors;

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

    public async Task<Result<TicketPayment, PurchaseError>> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        var concert = await concertModule.GetByIdAsync(purchaseParams.ConcertId);
        if (concert is null)
            return Result<TicketPayment, PurchaseError>.Failure(
                new PurchaseError.ConcertNotFound(purchaseParams.ConcertId));

        var validationResult = ticketValidator.CanPurchaseTickets(concert, purchaseParams.Quantity);
        if (validationResult.TryGetError(out var errors))
            return Result<TicketPayment, PurchaseError>.Failure(
                new PurchaseError.Invalid(CreateValidationErrors("purchase", errors)));

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

        return paymentResult.Match(
            payment => Result<TicketPayment, PurchaseError>.Success(new TicketPayment
            {
                RequiresAction = payment.RequiresAction,
                TransactionId = payment.TransactionId,
                ClientSecret = payment.ClientSecret,
                UserEmail = currentUser.Email
            }),
            error => Result<TicketPayment, PurchaseError>.Failure(ToPurchaseError(error)));
    }

    public async Task<TicketPayment> CompleteAsync(PurchaseComplete purchaseCompleteDto)
    {
        var concert = await concertModule.GetByIdAsync(purchaseCompleteDto.EntityId);
        if (concert is null)
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

    public async Task<Result<TicketCheckout, CheckoutError>> CheckoutAsync(int concertId, int quantity)
    {
        var concert = await concertModule.GetByIdAsync(concertId);
        if (concert is null)
            return Result<TicketCheckout, CheckoutError>.Failure(new CheckoutError.ConcertNotFound(concertId));

        var validationResult = ticketValidator.CanPurchaseTickets(concert, quantity);
        if (validationResult.TryGetError(out var errors))
            return Result<TicketCheckout, CheckoutError>.Failure(
                new CheckoutError.Invalid(CreateValidationErrors("checkout", errors)));

        var metadata = new Dictionary<string, string>
        {
            [PaymentMetadataKeys.Type] = TransactionTypes.Ticket,
            [PaymentMetadataKeys.ConcertId] = concert.Id.ToString(),
            [PaymentMetadataKeys.Quantity] = quantity.ToString(),
            [PaymentMetadataKeys.Amount] = Money.Gbp(concert.Price * quantity).ToMinorUnits().ToString(),
            [PaymentMetadataKeys.Currency] = "gbp"
        };

        var session = await customerPaymentClient.CreatePaymentSessionAsync(currentUser.GetId(), concert.Id, concert.PayeeOwnerId, metadata);

        return Result<TicketCheckout, CheckoutError>.Success(
            new TicketCheckout(session, concert.Price, concert.Id, quantity));
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

    private static ValidationErrors CreateValidationErrors(string field, IReadOnlyList<string> messages) =>
        new(new Dictionary<string, string[]> { [field] = messages.ToArray() });
}
