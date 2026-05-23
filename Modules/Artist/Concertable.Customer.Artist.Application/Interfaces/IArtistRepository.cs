namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistRepository
{
    Task<ArtistEntity?> GetByIdAsync(int artistId);
    Task AddAsync(ArtistEntity artist);
    Task SaveChangesAsync();
}
