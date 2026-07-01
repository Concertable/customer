using Concertable.Customer.Artist.Api.Responses;
using Concertable.Customer.Artist.Application.DTOs;

namespace Concertable.Customer.Artist.Api.Mappers;

internal static class ArtistResponseMappers
{
    public static ArtistDetailsResponse ToDetailsResponse(this ArtistDetails dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        About = dto.About,
        BannerUrl = dto.BannerUrl,
        Avatar = dto.Avatar,
        Rating = dto.Rating,
        Genres = dto.Genres,
        Email = dto.Email,
        County = dto.County,
        Town = dto.Town,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude
    };
}
