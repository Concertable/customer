using Concertable.Customer.Concert.Application.Interfaces;
using Concertable.Customer.Contracts;
using Concertable.Concert.Application.Interfaces;
using Concertable.Payment.Application.DTOs;
using Concertable.Payment.Application.Responses;
using Concertable.Payment.Contracts;
using Concertable.Payment.Domain;
using Concertable.Shared.Exceptions;
using FluentResults;
using Microsoft.Extensions.Logging;
using B2BConcertEntity = Concertable.Concert.Domain.ConcertEntity;
using CustomerConcertEntity = Concertable.Customer.Concert.Domain.ConcertEntity;
using IB2BConcertRepository = Concertable.Concert.Application.Interfaces.IConcertRepository;
using ICustomerConcertRepository = Concertable.Customer.Concert.Application.Interfaces.IConcertRepository;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal class TicketService : ITicketService
{
    private readonly ITicketRepository ticketRepository;
    private readonly ITicketValidator ticketValidator;
    private readonly IEmailService emailService;
    private readonly IQrCodeService qrCodeService;
    private readonly ICurrentUser currentUser;
    private readonly IB2BConcertRepository b2bConcertRepository;
    private readonly ICustomerConcertRepository customerConcertRepository;
    private readonly IContractLoader contractLoader;
    private readonly ITicketPayee ticketPayee;
    private readonly ICustomerPaymentModule customerPaymentModule;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<TicketService> logger;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketValidator ticketValidator,
        IEmailService emailService,
        IQrCodeService qrCodeService,
        ICurrentUser currentUser,
        IB2BConcertRepository b2bConcertRepository,
        ICustomerConcertRepository customerConcertRepository,
        IContractLoader contractLoader,
        ITicketPayee ticketPayee,
        ICustomerPaymentModule customerPaymentModule,
        TimeProvider timeProvider,
        ILogger<TicketService> logger)
    {
        this.ticketRepository = ticketRepository;
        this.ticketValidator = ticketValidator;
        this.emailService = emailService;
        this.qrCodeService = qrCodeService;
        this.currentUser = currentUser;
        this.b2bConcertRepository = b2bConcertRepository;
        this.customerConcertRepository = customerConcertRepository;
        this.contractLoader = contractLoader;
        this.ticketPayee = ticketPayee;
        this.customerPaymentModule = customerPaymentModule;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<Result<TicketPaymentResponse>> PurchaseAsync(TicketPurchaseParams purchaseParams)
    {
        if (currentUser.GetRole() != Role.Customer)
            throw new ForbiddenException("Only Customers can buy tickets");

        var customerConcert = await customerConcertRepository.GetByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert not found");

        var validationResult = ticketValidator.CanPurchaseTickets(customerConcert, purchaseParams.Quantity);
        if (validationResult.IsFailed)
            throw new BadRequestException(validationResult.Errors);

        // Payee routing still needs B2B's contract + concert navs — Phase 1 cross-ref.
        var b2bConcert = await b2bConcertRepository.GetFullByIdAsync(purchaseParams.ConcertId)
            ?? throw new NotFoundException("Concert not found");
        var contract = await contractLoader.LoadByConcertIdAsync(purchaseParams.ConcertId);
        var payeeUserId = ticketPayee.Resolve(b2bConcert, contract);

        logger.LogInformation(
            "Routing ticket revenue for concert {ConcertId} ({ContractType}) to {PayeeUserId}: {Quantity} x {Price} {Currency}",
            purchaseParams.ConcertId, contract.ContractType, payeeUserId, purchaseParams.Quantity, customerConcert.Price, "GBP");

        var metadata = new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = purchaseParams.ConcertId.ToString(),
            ["quantity"] = purchaseParams.Quantity.ToString()
        };

        var paymentResult = await customerPaymentModule.PayAsync(
            currentUser.GetId(), payeeUserId,
            customerConcert.Price * purchaseParams.Quantity,
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
        var customerConcert = await customerConcertRepository.GetByIdAsync(purchaseCompleteDto.EntityId);
        if (customerConcert is null)
            return Result.Fail("Concert not found");

        // Snapshot fields (Name/Venue/Artist) sourced from B2B nav chain — Phase 1 cross-ref.
        var b2bConcert = await b2bConcertRepository.GetFullByIdAsync(purchaseCompleteDto.EntityId);
        if (b2bConcert is null)
            return Result.Fail("Concert not found");

        int quantity = purchaseCompleteDto.Quantity ?? 1;
        var tickets = new List<TicketEntity>();

        try
        {
            for (int i = 0; i < quantity; i++)
            {
                var ticket = BuildTicket(purchaseCompleteDto.FromUserId, b2bConcert, customerConcert);
                await ticketRepository.AddAsync(ticket);
                tickets.Add(ticket);
            }

            customerConcert.DecrementAvailability(quantity);
            await customerConcertRepository.SaveChangesAsync();
            await ticketRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return Result.Fail("Failed to create ticket. Please contact support.");
        }

        var ticketIds = tickets.Select(t => t.Id).ToList();
        await emailService.SendTicketsToEmailAsync(purchaseCompleteDto.FromEmail, ticketIds);

        return Result.Ok(new TicketPaymentResponse
        {
            TicketIds = ticketIds,
            ConcertId = purchaseCompleteDto.EntityId,
            PurchaseDate = tickets[0].PurchaseDate,
            Amount = customerConcert.Price,
            Currency = "GBP",
            UserEmail = purchaseCompleteDto.FromEmail
        });
    }

    public async Task<Result<TicketCheckout>> CheckoutAsync(int concertId, int quantity)
    {
        if (currentUser.GetRole() != Role.Customer)
            throw new ForbiddenException("Only Customers can buy tickets");

        var customerConcert = await customerConcertRepository.GetByIdAsync(concertId)
            ?? throw new NotFoundException("Concert not found");

        var validationResult = ticketValidator.CanPurchaseTickets(customerConcert, quantity);
        if (validationResult.IsFailed)
            return Result.Fail(validationResult.Errors);

        var b2bConcert = await b2bConcertRepository.GetFullByIdAsync(concertId)
            ?? throw new NotFoundException("Concert not found");
        var contract = await contractLoader.LoadByConcertIdAsync(concertId);
        var payeeUserId = ticketPayee.Resolve(b2bConcert, contract);

        var metadata = new Dictionary<string, string>
        {
            ["type"] = TransactionTypes.Ticket,
            ["concertId"] = concertId.ToString(),
            ["toUserId"] = payeeUserId.ToString(),
            ["quantity"] = quantity.ToString(),
            ["amount"] = ((long)(customerConcert.Price * quantity * 100)).ToString(),
            ["currency"] = "gbp"
        };

        var session = await customerPaymentModule.CreatePaymentSessionAsync(currentUser.GetId(), metadata);

        return Result.Ok(new TicketCheckout(session, customerConcert.Price, concertId, quantity));
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

    private TicketEntity BuildTicket(Guid userId, B2BConcertEntity b2bConcert, CustomerConcertEntity customerConcert)
    {
        var ticketId = Guid.CreateVersion7();
        var qrCode = qrCodeService.GenerateFromTicketId(ticketId);
        return TicketEntity.Create(
            ticketId,
            userId,
            customerConcert.Id,
            qrCode,
            timeProvider.GetUtcNow().DateTime,
            b2bConcert.Name,
            customerConcert.Price,
            customerConcert.Period,
            b2bConcert.Booking.Application.Opportunity.Venue.Name,
            b2bConcert.Booking.Application.Artist.Name);
    }
}
