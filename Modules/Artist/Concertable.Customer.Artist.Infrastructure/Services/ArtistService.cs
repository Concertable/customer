using Concertable.Customer.Artist.Application.DTOs;

namespace Concertable.Customer.Artist.Infrastructure.Services;

internal sealed class ArtistService : IArtistService
{
    private readonly IArtistReadRepository repository;

    public ArtistService(IArtistReadRepository repository)
    {
        this.repository = repository;
    }

    public Task<ArtistDetails?> GetDetailsByIdAsync(int artistId) =>
        repository.GetDetailsByIdAsync(artistId);
}
