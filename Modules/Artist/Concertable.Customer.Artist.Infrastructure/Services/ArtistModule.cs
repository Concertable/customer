using Concertable.Customer.Artist.Application.Interfaces;
using Concertable.Customer.Artist.Contracts;

namespace Concertable.Customer.Artist.Infrastructure.Services;

internal sealed class ArtistModule : IArtistModule
{
    private readonly IArtistReadRepository artistRepository;

    public ArtistModule(IArtistReadRepository artistRepository)
    {
        this.artistRepository = artistRepository;
    }

    public Task<ArtistSummary?> GetSummaryAsync(int artistId, CancellationToken ct = default) =>
        artistRepository.GetSummaryAsync(artistId);
}
