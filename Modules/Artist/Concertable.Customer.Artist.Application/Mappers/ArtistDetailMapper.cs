using Concertable.Customer.Artist.Application.Dtos;
using Concertable.Customer.Artist.Domain.Entities;

namespace Concertable.Customer.Artist.Application.Mappers;

internal static class ArtistDetailMapper
{
    public static ArtistDetail ToDetailDto(this ArtistEntity artist) => new(
        artist.Id,
        artist.Name,
        artist.About,
        artist.BannerUrl,
        artist.Avatar,
        artist.AverageRating,
        artist.Genres.Select(g => g.Genre).ToArray(),
        artist.Email,
        artist.County,
        artist.Town,
        artist.Latitude,
        artist.Longitude);
}
