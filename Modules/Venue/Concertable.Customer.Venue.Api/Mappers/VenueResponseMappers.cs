using Concertable.Customer.Venue.Api.Responses;
using Concertable.Customer.Venue.Application.DTOs;

namespace Concertable.Customer.Venue.Api.Mappers;

internal static class VenueResponseMappers
{
    public static VenueDetailsResponse ToDetailsResponse(this VenueDetails dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl,
        Avatar = dto.Avatar,
        Rating = dto.Rating,
        County = dto.County,
        Town = dto.Town,
        Email = dto.Email,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude
    };
}
