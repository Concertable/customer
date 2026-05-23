using Concertable.Customer.Venue.Application.Dtos;

namespace Concertable.Customer.Venue.Application.Mappers;

internal static class VenueDetailMapper
{
    public static VenueDetailDto ToDetailDto(this VenueEntity venue) => new(
        venue.Id,
        venue.Name,
        venue.About,
        venue.BannerUrl,
        venue.Avatar,
        venue.AverageRating,
        venue.County,
        venue.Town,
        venue.Email,
        venue.Latitude,
        venue.Longitude);
}
