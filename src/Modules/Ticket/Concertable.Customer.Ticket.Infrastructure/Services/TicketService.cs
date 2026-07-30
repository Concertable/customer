using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Kernel.Identity;
using Concertable.Kernel.Exceptions;
using Concertable.Messaging.Contracts;
using Concertable.Shared.QrCode.Application;
using FluentResults;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IQrCodeGenerator qrCodeGenerator;
    private readonly ICurrentUser currentUser;
    private readonly IConcertModule concertModule;
    private readonly ICustomerPaymentClient customerPaymentClient;
    private readonly IOutboxUnitOfWorkBehavior outboxBehavior;
    private readonly IBus bus;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IQrCodeGenerator qrCodeGenerator,
        ICurrentUser currentUser,
        IConcertModule concertModule,
        ICustomerPaymentClient customerPaymentClient,
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

    public async Task<Result<TicketPayment>> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        var concert = await concertModule.GetByIdAsync(purchaseParams.ConcertId)
            .OrNotFound();

        var validationResult = ticketValidator.CanPurchaseTickets(concert, purchaseParams.Quantity);
        if (validationResult.IsFailed)
            throw new BadRequestException(validationResult.Errors);

        var metadata = new Dictionary<string, string>
        {
            [PaymentMetadataKeys.Type] = TransactionTypes.Ticket,
            [PaymentMetadataKeys.ConcertId] = concert.Id.ToString(),
            [PaymentMetadataKeys.Quantity] = purchaseParams.Quantity.ToString()
        };

        var paymentResult = await customerPaymentClient.PayAsync(
            currentUser.GetId(), concert.Id, concert.PayeeOwnerId,
            concert.Price * purchaseParams.Quantity,
            metadata,
            purchaseParams.PaymentMethodId);

        if (paymentResult.IsFailed)
            return Result.Fail(paymentResult.Errors);

        return Result.Ok(new TicketPayment
        {
            RequiresAction = paymentResult.Value.RequiresAction,
            TransactionId = paymentResult.Value.TransactionId,
            ClientSecret = paymentResult.Value.ClientSecret,
            UserEmail = currentUser.Email
        });
    }

    public async Task<Result<TicketPayment>> CompleteAsync(PurchaseComplete purchaseCompleteDto)
    {
        var concert = await concertModule.GetByIdAsync(purchaseCompleteDto.EntityId);
        if (concert is null)
            return Result.Fail("Concert not found");

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

        return Result.Ok(new TicketPayment
        {
            TicketIds = ticketIds,
            ConcertId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = concert.Price,
            Currency = "GBP",
            UserEmail = purchaseCompleteDto.FromEmail
        });
    }

    public async Task<Result<TicketCheckout>> CheckoutAsync(int concertId, int quantity)
    {
        var concert = await concertModule.GetByIdAsync(concertId)
            .OrNotFound();

        var validationResult = ticketValidator.CanPurchaseTickets(concert, quantity);
        if (validationResult.IsFailed)
            return Result.Fail(validationResult.Errors);

        var metadata = new Dictionary<string, string>
        {
            [PaymentMetadataKeys.Type] = TransactionTypes.Ticket,
            [PaymentMetadataKeys.ConcertId] = concert.Id.ToString(),
            [PaymentMetadataKeys.Quantity] = quantity.ToString(),
            [PaymentMetadataKeys.Amount] = ((long)(concert.Price * quantity * 100)).ToString(),
            [PaymentMetadataKeys.Currency] = "gbp"
        };

        var session = await customerPaymentClient.CreatePaymentSessionAsync(currentUser.GetId(), concert.Id, concert.PayeeOwnerId, metadata);

        return Result.Ok(new TicketCheckout(session, concert.Price, concert.Id, quantity));
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
}
