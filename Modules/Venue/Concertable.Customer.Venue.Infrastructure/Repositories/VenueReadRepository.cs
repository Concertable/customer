using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal sealed class VenueReadRepository : ReadRepository<VenueEntity>, IVenueReadRepository
{
    public VenueReadRepository(VenueDbContext context) : base(context) { }

    public Task<VenueDetails?> GetDetailsByIdAsync(int venueId) =>
        context.Venues
            .Where(v => v.Id == venueId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
