using Concertable.Customer.Artist.Application.DTOs;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistService
{
    Task<ArtistDetails?> GetDetailsByIdAsync(int artistId);
}
