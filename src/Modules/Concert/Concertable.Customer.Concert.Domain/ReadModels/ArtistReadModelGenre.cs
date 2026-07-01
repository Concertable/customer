using Concertable.Contracts;

namespace Concertable.Customer.Concert.Domain.ReadModels;

public sealed class ArtistReadModelGenre
{
    public int ArtistReadModelId { get; set; }
    public Genre Genre { get; set; }
    public ArtistReadModel Artist { get; set; } = null!;
}
