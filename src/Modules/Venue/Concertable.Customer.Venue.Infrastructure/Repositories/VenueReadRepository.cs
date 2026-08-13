using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Customer.Venue.Infrastructure.Mappers;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal sealed class VenueReadRepository : ReadRepository<VenueEntity, int>, IVenueReadRepository
{
    public VenueReadRepository(IReadDbContext context) : base(context) { }

    public Task<VenueDetails?> GetDetailsByIdAsync(int venueId) =>
        base.context.Query<VenueEntity>()
            .Where(v => v.Id == venueId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
