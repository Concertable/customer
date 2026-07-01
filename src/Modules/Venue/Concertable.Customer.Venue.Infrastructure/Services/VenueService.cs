using Concertable.Customer.Venue.Application.DTOs;

namespace Concertable.Customer.Venue.Infrastructure.Services;

internal sealed class VenueService : IVenueService
{
    private readonly IVenueReadRepository repository;

    public VenueService(IVenueReadRepository repository)
    {
        this.repository = repository;
    }

    public Task<VenueDetails?> GetDetailsByIdAsync(int venueId) =>
        repository.GetDetailsByIdAsync(venueId);
}
