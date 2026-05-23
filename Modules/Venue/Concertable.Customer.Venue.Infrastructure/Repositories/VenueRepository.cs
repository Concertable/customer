using Concertable.Customer.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal class VenueReadRepository : IVenueReadRepository
{
    private readonly VenueDbContext context;

    public VenueReadRepository(VenueDbContext context)
    {
        this.context = context;
    }

    public Task<VenueReadModel?> GetByIdAsync(int venueId) =>
        context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);
}
