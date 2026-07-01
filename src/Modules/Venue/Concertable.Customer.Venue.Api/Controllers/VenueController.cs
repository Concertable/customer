using Concertable.Customer.Venue.Api.Mappers;
using Concertable.Customer.Venue.Api.Responses;
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
    public async Task<ActionResult<VenueDetailsResponse>> GetDetailsById(int id)
    {
        var venue = await venueService.GetDetailsByIdAsync(id);
        return venue is null ? NotFound() : Ok(venue.ToDetailsResponse());
    }
}
