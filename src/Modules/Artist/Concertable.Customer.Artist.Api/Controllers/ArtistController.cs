using Concertable.Customer.Artist.Api.Mappers;
using Concertable.Customer.Artist.Api.Responses;
using Concertable.Customer.Artist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Artist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal sealed class ArtistController : ControllerBase
{
    private readonly IArtistService artistService;

    public ArtistController(IArtistService artistService)
    {
        this.artistService = artistService;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ArtistDetailsResponse>> GetDetailsById(int id)
    {
        var artist = await artistService.GetDetailsByIdAsync(id);
        return artist is null ? NotFound() : Ok(artist.ToDetailsResponse());
    }
}
