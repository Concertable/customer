using Concertable.Customer.Venue.Application.Interfaces;
using Concertable.Customer.Venue.Contracts;

namespace Concertable.Customer.Venue.Infrastructure.Services;

internal sealed class VenueModule : IVenueModule
{
    private readonly IVenueReadRepository venueRepository;

    public VenueModule(IVenueReadRepository venueRepository)
    {
        this.venueRepository = venueRepository;
    }

    public Task<VenueSummary?> GetSummaryAsync(int venueId, CancellationToken ct = default) =>
        venueRepository.GetSummaryAsync(venueId);
}
