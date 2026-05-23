using Concertable.Customer.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal class VenueRepository : IVenueRepository
{
    private readonly VenueDbContext context;

    public VenueRepository(VenueDbContext context)
    {
        this.context = context;
    }

    public Task<VenueEntity?> GetByIdAsync(int venueId) =>
        context.Venues.FirstOrDefaultAsync(v => v.Id == venueId);

    public async Task AddAsync(VenueEntity venue) =>
        await context.Venues.AddAsync(venue);

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
