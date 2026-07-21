using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Kernel.Identity;
using Concertable.Kernel.Exceptions;
using Concertable.Messaging.Contracts;
using Concertable.Messaging.Infrastructure.Outbox;
using FluentResults;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUser currentUser;
    private readonly IConcertModule concertModule;
    private readonly ICustomerPaymentClient customerPaymentClient;
    private readonly TicketDbContext context;
    private readonly IDbContextAccessor contextAccessor;
    private readonly IBus bus;
    private readonly TimeProvider timeProvider;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IQrCodeService qrCodeService,
        ICurrentUser currentUser,
        IConcertModule concertModule,
        ICustomerPaymentClient customerPaymentClient,
        TicketDbContext context,
        IDbContextAccessor contextAccessor,
        IBus bus,
        TimeProvider timeProvider)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.qrCodeService = qrCodeService;
        this.currentUser = currentUser;
        this.concertModule = concertModule;
        this.customerPaymentClient = customerPaymentClient;
        this.context = context;
        this.contextAccessor = contextAccessor;
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
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["quantity"] = purchaseParams.Quantity.ToString()
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

        for (int i = 0; i < quantity; i++)
        {
            var ticket = BuildTicket(purchaseCompleteDto.FromUserId, concert);
            await ticketRepository.AddAsync(ticket);
            tickets.Add(ticket);
        }

        var ticketIds = tickets.Select(t => t.Id).ToList();

        try
        {
            contextAccessor.Context = context;
            await bus.SendAsync(new SendTicketEmailCommand(purchaseCompleteDto.FromEmail, ticketIds));
            await context.SaveChangesAsync();
        }
        finally
        {
            contextAccessor.Context = null;
        }

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
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["quantity"] = quantity.ToString(),
            ["amount"] = ((long)(concert.Price * quantity * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await customerPaymentClient.CreatePaymentSessionAsync(currentUser.GetId(), concert.Id, concert.PayeeOwnerId, metadata);

        return Result.Ok(new TicketCheckout(session, concert.Price, concert.Id, quantity));
    }

    public async Task<IEnumerable<TicketDto>> GetUserUpcomingAsync()
    {
        var tickets = await ticketRepository.GetUpcomingByUserIdAsync(currentUser.GetId());
        return tickets.ToDtos(currentUser.Email ?? string.Empty);
    }

    public async Task<IEnumerable<TicketDto>> GetUserHistoryAsync()
    {
        var tickets = await ticketRepository.GetHistoryByUserIdAsync(currentUser.GetId());
        return tickets.ToDtos(currentUser.Email ?? string.Empty);
    }

    private TicketEntity BuildTicket(Guid userId, ConcertDto concert)
    {
        var ticketId = Guid.CreateVersion7();
        var qrCode = qrCodeService.GenerateFromTicketId(ticketId);
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
