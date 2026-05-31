using Concertable.Customer.Venue.Domain.Entities;
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

    public Task<VenueEntity?> GetByIdAsync(int venueId) =>
        context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);
}
