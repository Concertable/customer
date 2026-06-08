using Concertable.Customer.Venue.Contracts;
using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Venue.Infrastructure.Mappers;

internal static class QueryableVenueMappers
{
    public static IQueryable<VenueSummary> ToSummary(this IQueryable<VenueEntity> query) =>
        query.Select(v => new VenueSummary(v.Id, v.Name, v.County, v.Town, v.Latitude, v.Longitude));
}
