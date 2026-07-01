using Concertable.Kernel;

namespace Concertable.Customer.Concert.Domain.ReadModels;

/// <summary>
/// Denormalized read model of a venue, owned by the Concert module and kept in sync
/// via <c>VenueChangedEvent</c> projections so concert details can be served from a
/// single query without crossing into the Venue module's DB context.
/// </summary>
public sealed class VenueReadModel : IIdEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
