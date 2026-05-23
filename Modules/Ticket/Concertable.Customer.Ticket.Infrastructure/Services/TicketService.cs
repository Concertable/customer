using Concertable.Customer.Concert.Application.Interfaces;
using Concertable.Customer.Concert.Domain;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Application.Responses;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly ITicketEmailSender ticketEmailSender;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUser currentUser;
    private readonly IConcertReadRepository concertRepository;
    private readonly ICustomerPaymentClient customerPaymentClient;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<TicketService> logger;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        ITicketEmailSender ticketEmailSender,
        IQrCodeService qrCodeService,
        ICurrentUser currentUser,
        IConcertReadRepository concertRepository,
        ICustomerPaymentClient customerPaymentClient,
        TimeProvider timeProvider,
        ILogger<TicketService> logger)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.ticketEmailSender = ticketEmailSender;
        this.qrCodeService = qrCodeService;
        this.currentUser = currentUser;
        this.concertRepository = concertRepository;
        this.customerPaymentClient = customerPaymentClient;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        var concert = await concertRepository.GetByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert not found");

        var validationResult = ticketValidator.CanPurchaseTickets(concert, purchaseParams.Quantity);
        if (validationResult.IsFailed)
            throw new BadRequestException(validationResult.Errors);

        logger.LogInformation(
            "Routing ticket revenue for concert {ConcertId} ({ContractType}) to {PayeeUserId}: {Quantity} x {Price} {Currency}",
            concert.Id, concert.ContractType, concert.PayeeUserId, purchaseParams.Quantity, concert.Price, "GBP");

        var metadata = new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["quantity"] = purchaseParams.Quantity.ToString()
        };

        var paymentResult = await customerPaymentClient.PayAsync(
            currentUser.GetId(), concert.PayeeUserId,
            concert.Price * purchaseParams.Quantity,
            metadata,
            purchaseParams.PaymentMethodId);

        if (paymentResult.IsFailed)
            return Result.Fail(paymentResult.Errors);

        return Result.Ok(new TicketPaymentResponse
        {
            RequiresAction = paymentResult.Value.RequiresAction,
            TransactionId = paymentResult.Value.TransactionId,
            ClientSecret = paymentResult.Value.ClientSecret,
            UserEmail = currentUser.Email
        });
    }

    public async Task<Result<TicketPaymentResponse>> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto)
    {
        var concert = await concertRepository.GetByIdAsync(purchaseCompleteDto.EntityId);
        if (concert is null)
            return Result.Fail("Concert not found");

        int quantity = purchaseCompleteDto.Quantity ?? 1;
        var tickets = new List<TicketEntity>();

        try
        {
            for (int i = 0; i < quantity; i++)
            {
                var ticket = BuildTicket(purchaseCompleteDto.FromUserId, concert);
                await ticketRepository.AddAsync(ticket);
                tickets.Add(ticket);
            }

            concert.DecrementAvailability(quantity);
            await concertRepository.SaveChangesAsync();
            await ticketRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return Result.Fail("Failed to create ticket. Please contact support.");
        }

        var ticketIds = tickets.Select(t => t.Id).ToList();
        await ticketEmailSender.SendTicketsAsync(purchaseCompleteDto.FromEmail, ticketIds);

        return Result.Ok(new TicketPaymentResponse
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
        var concert = await concertRepository.GetByIdAsync(concertId)
            ?? throw new NotFoundException("Concert not found");

        var validationResult = ticketValidator.CanPurchaseTickets(concert, quantity);
        if (validationResult.IsFailed)
            return Result.Fail(validationResult.Errors);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concert.Id.ToString(),
            ["toUserId"] = concert.PayeeUserId.ToString(),
            ["quantity"] = quantity.ToString(),
            ["amount"] = ((long)(concert.Price * quantity * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await customerPaymentClient.CreatePaymentSessionAsync(currentUser.GetId(), metadata);

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

    private TicketEntity BuildTicket(Guid userId, ConcertReadModel concert)
    {
        var ticketId = Guid.CreateVersion7();
        var qrCode = qrCodeService.GenerateFromTicketId(ticketId);
        return TicketEntity.Create(
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
