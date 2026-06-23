using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Venue.Infrastructure.Mappers;

internal static class QueryableVenueMappers
{
    public static IQueryable<VenueDetails> ToDetails(this IQueryable<VenueEntity> query) =>
        query.Select(v => new VenueDetails(
            v.Id,
            v.Name,
            v.About,
            v.BannerUrl,
            v.Avatar,
            v.AverageRating,
            v.Address.County,
            v.Address.Town,
            v.Email,
            v.Latitude,
            v.Longitude));
}
