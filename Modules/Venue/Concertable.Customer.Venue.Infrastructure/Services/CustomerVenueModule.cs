using Concertable.Customer.Venue.Contracts;
using Concertable.Customer.Venue.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Services;

internal sealed class CustomerVenueModule : ICustomerVenueModule
{
    private readonly VenueDbContext context;

    public CustomerVenueModule(VenueDbContext context)
    {
        this.context = context;
    }

    public Task<VenueSummary?> GetSummaryAsync(int venueId, CancellationToken ct = default) =>
        context.Venues
            .Where(v => v.Id == venueId)
            .Select(v => new VenueSummary(v.Id, v.Name, v.County, v.Town, v.Latitude, v.Longitude))
            .FirstOrDefaultAsync(ct);
}
