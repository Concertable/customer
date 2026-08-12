using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Customer.Venue.Infrastructure.Mappers;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal sealed class VenueReadRepository : ReadRepository<VenueEntity>, IVenueReadRepository
{
    public VenueReadRepository(IReadDbContext context) : base(context) { }

    public Task<VenueDetails?> GetDetailsByIdAsync(int venueId) =>
        context.Query<VenueEntity>()
            .Where(v => v.Id == venueId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
