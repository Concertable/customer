using Concertable.Customer.Concert.Application.Dtos;
using Concertable.Customer.Concert.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Concert.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal sealed class ConcertController : ControllerBase
{
    private readonly IConcertService concertService;

    public ConcertController(IConcertService concertService)
    {
        this.concertService = concertService;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ConcertDetail>> GetById(int id, CancellationToken ct)
    {
        var concert = await concertService.GetByIdAsync(id, ct);
        return concert is null ? NotFound() : Ok(concert);
    }
}
