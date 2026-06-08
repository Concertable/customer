using Concertable.Customer.Artist.Application.Dtos;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistService
{
    Task<ArtistDetail?> GetByIdAsync(int artistId);
}
