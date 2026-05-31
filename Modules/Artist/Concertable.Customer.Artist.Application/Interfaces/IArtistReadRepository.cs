using Concertable.Customer.Artist.Domain.Entities;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistReadRepository
{
    Task<ArtistEntity?> GetByIdAsync(int artistId);
}
