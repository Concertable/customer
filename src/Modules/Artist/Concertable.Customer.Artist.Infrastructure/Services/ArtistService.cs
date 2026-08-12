using Concertable.Customer.Artist.Application.DTOs;
using Reunion;

namespace Concertable.Customer.Artist.Infrastructure.Services;

internal sealed class ArtistService : IArtistService
{
    private readonly IArtistReadRepository repository;

    public ArtistService(IArtistReadRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Option<ArtistDetails>> GetDetailsByIdAsync(int artistId) =>
        await repository.GetDetailsByIdAsync(artistId);
}
