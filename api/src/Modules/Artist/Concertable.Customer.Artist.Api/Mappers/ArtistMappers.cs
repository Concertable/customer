using Concertable.Customer.Artist.Api.Responses;
using Concertable.Customer.Artist.Application.DTOs;

namespace Concertable.Customer.Artist.Api.Mappers;

internal static class ArtistMappers
{
    extension(ArtistDetails dto)
    {
        public DetailsResponse ToDetailsResponse() => new()
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
}
