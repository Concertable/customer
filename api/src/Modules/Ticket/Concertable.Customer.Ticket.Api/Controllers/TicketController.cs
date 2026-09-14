using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.User.Api.Authorization;
using Concertable.Kernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Reunion.AspNetCore.Mvc;
using Reunion.Validation;

namespace Concertable.Customer.Ticket.Api.Controllers;

[Customer]
[ApiController]
[Route("api/[controller]")]
internal sealed class TicketController : ControllerBase
{
    private readonly ITicketService ticketService;
    private readonly ITicketValidator ticketValidator;

    public TicketController(ITicketService ticketService, ITicketValidator ticketValidator)
    {
        this.ticketService = ticketService;
        this.ticketValidator = ticketValidator;
    }

    [EnableRateLimiting("purchase")]
    [HttpPost("purchase")]
    public async Task<ActionResult<TicketPayment>> Purchase([FromBody] TicketPurchaseParams purchaseParams)
    {
        var result = await ticketService.PurchaseAsync(purchaseParams);

        return result.ToOkOrProblem();
    }

    [EnableRateLimiting("purchase")]
    [HttpPost("checkout")]
    public async Task<ActionResult<TicketCheckout>> Checkout([FromBody] TicketCheckoutRequest request)
    {
        var result = await ticketService.CheckoutAsync(request.ConcertId, request.Quantity);

        return result.ToOkOrProblem();
    }

    [HttpGet("upcoming/user")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetUserUpcoming()
    {
        return Ok(await ticketService.GetUserUpcomingAsync());
    }

    [HttpGet("history/user")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetUserHistory()
    {
        return Ok(await ticketService.GetUserHistoryAsync());
    }

    [HttpGet("concert/{concertId}/eligibility")]
    public async Task<ActionResult<bool>> CanPurchase(int concertId)
    {
        var result = await ticketValidator.CanBePurchasedAsync(concertId);
        return result.ToOkOrProblem((ValidationResult validation) => validation.IsValid);
    }
}
