using Concertable.Customer.Concert.Api.Mappers;
using Concertable.Customer.Concert.Api.Responses;
using Concertable.Customer.Concert.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Reunion.AspNetCore.Mvc;

namespace Concertable.Customer.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public-read")]
internal sealed class ConcertController : ControllerBase
{
    private readonly IConcertService concertService;

    public ConcertController(IConcertService concertService)
    {
        this.concertService = concertService;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<DetailsResponse>> GetDetailsById(int id, CancellationToken ct)
    {
        var concert = await concertService.GetDetailsByIdAsync(id, ct);
        return concert.ToOkOr(value => value.ToDetailsResponse(), NotFound);
    }
}
