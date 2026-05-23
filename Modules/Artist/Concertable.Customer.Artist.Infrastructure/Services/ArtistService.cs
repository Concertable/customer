using Concertable.Customer.Artist.Application.Dtos;
using Concertable.Customer.Artist.Application.Mappers;

namespace Concertable.Customer.Artist.Infrastructure.Services;

internal class ArtistService : IArtistService
{
    private readonly IArtistRepository repository;

    public ArtistService(IArtistRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ArtistDetailDto?> GetByIdAsync(int artistId)
    {
        var artist = await repository.GetByIdAsync(artistId);
        return artist?.ToDetailDto();
    }
}
