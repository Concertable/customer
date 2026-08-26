using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Mappers;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal sealed class VenueReadRepository : QueryableReadRepository<VenueEntity, int>, IVenueReadRepository
{
    private readonly IVenueReadDbContext context;

    public VenueReadRepository(IVenueReadDbContext context) : base(context.Venues)
    {
        this.context = context;
    }

    public Task<VenueDetails?> GetDetailsByIdAsync(int venueId) =>
        context.Venues
            .Where(v => v.Id == venueId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
