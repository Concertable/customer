using Concertable.Customer.Venue.Application.Dtos;
using Concertable.Customer.Venue.Application.Mappers;

namespace Concertable.Customer.Venue.Infrastructure.Services;

internal class VenueService : IVenueService
{
    private readonly IVenueRepository repository;

    public VenueService(IVenueRepository repository)
    {
        this.repository = repository;
    }

    public async Task<VenueDetailDto?> GetByIdAsync(int venueId)
    {
        var venue = await repository.GetByIdAsync(venueId);
        return venue?.ToDetailDto();
    }
}
