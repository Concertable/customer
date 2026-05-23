using Concertable.Customer.Artist.Application.Dtos;
using Concertable.Customer.Artist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concertable.Customer.Artist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
internal class ArtistController : ControllerBase
{
    private readonly IArtistService artistService;

    public ArtistController(IArtistService artistService)
    {
        this.artistService = artistService;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ArtistDetailDto>> GetById(int id)
    {
        var artist = await artistService.GetByIdAsync(id);
        return artist is null ? NotFound() : Ok(artist);
    }
}
