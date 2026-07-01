using Concertable.Kernel;

namespace Concertable.Customer.Concert.Domain.ReadModels;

/// <summary>
/// Denormalized read model of an artist, owned by the Concert module and kept in sync
/// via <c>ArtistChangedEvent</c> / <c>ArtistRatingUpdatedEvent</c> projections so concert
/// details can be served from a single query without crossing into the Artist module's DB context.
/// </summary>
public sealed class ArtistReadModel : IIdEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Avatar { get; set; } = null!;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Address Address { get; set; } = null!;
    public ICollection<ArtistReadModelGenre> Genres { get; set; } = [];
}
