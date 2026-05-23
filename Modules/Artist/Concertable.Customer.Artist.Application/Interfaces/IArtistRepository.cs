namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistReadRepository
{
    Task<ArtistReadModel?> GetByIdAsync(int artistId);
}
