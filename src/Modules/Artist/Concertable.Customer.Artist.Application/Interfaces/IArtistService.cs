using Concertable.Customer.Artist.Application.DTOs;
using Reunion;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistService
{
    Task<Option<ArtistDetails>> GetDetailsByIdAsync(int artistId);
}
