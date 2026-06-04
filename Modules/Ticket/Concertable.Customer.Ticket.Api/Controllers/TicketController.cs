using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.User.Api.Authorization;
using Concertable.Kernel;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("purchase")]
    public async Task<ActionResult<TicketPayment>> Purchase([FromBody] TicketPurchaseParams purchaseParams)
    {
        var result = await ticketService.PurchaseAsync(purchaseParams);

        if (result.IsFailed)
            return BadRequest(result.Errors.SelectMessages());

        return Ok(result.Value);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<TicketCheckout>> Checkout([FromBody] TicketCheckoutRequest request)
    {
        var result = await ticketService.CheckoutAsync(request.ConcertId, request.Quantity);

        if (result.IsFailed)
            return BadRequest(result.Errors.SelectMessages());

        return Ok(result.Value);
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
        return Ok(result.IsSuccess);
    }
}
