using Concertable.Customer.Artist.Application.Dtos;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistService
{
    Task<ArtistDetailDto?> GetByIdAsync(int artistId);
}
