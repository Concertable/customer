using Concertable.Customer.Venue.Application.Dtos;
using Concertable.Customer.Venue.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Venue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal sealed class VenueController : ControllerBase
{
    private readonly IVenueService venueService;

    public VenueController(IVenueService venueService)
    {
        this.venueService = venueService;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<VenueDetailDto>> GetById(int id)
    {
        var venue = await venueService.GetByIdAsync(id);
        return venue is null ? NotFound() : Ok(venue);
    }
}
