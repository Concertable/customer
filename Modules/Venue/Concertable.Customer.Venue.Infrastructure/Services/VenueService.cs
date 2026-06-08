using Concertable.Customer.Venue.Application.Dtos;
using Concertable.Customer.Venue.Application.Mappers;

namespace Concertable.Customer.Venue.Infrastructure.Services;

internal sealed class VenueService : IVenueService
{
    private readonly IVenueReadRepository repository;

    public VenueService(IVenueReadRepository repository)
    {
        this.repository = repository;
    }

    public async Task<VenueDetail?> GetByIdAsync(int venueId)
    {
        var venue = await repository.GetByIdAsync(venueId);
        return venue?.ToDetailDto();
    }
}
