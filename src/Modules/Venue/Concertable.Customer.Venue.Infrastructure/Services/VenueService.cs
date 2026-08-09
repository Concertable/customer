using Concertable.Customer.Venue.Application.DTOs;
using Reunion;

namespace Concertable.Customer.Venue.Infrastructure.Services;

internal sealed class VenueService : IVenueService
{
    private readonly IVenueReadRepository repository;

    public VenueService(IVenueReadRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Option<VenueDetails>> GetDetailsByIdAsync(int venueId)
    {
        var venue = await repository.GetDetailsByIdAsync(venueId);
        return venue.ToOption();
    }
}
