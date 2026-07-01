using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Domain.Entities;

namespace Concertable.Customer.Artist.Infrastructure.Mappers;

internal static class QueryableArtistMappers
{
    public static IQueryable<ArtistDetails> ToDetails(this IQueryable<ArtistEntity> query) =>
        query.Select(a => new ArtistDetails(
            a.Id,
            a.Name,
            a.About,
            a.BannerUrl,
            a.Avatar,
            a.AverageRating,
            a.Genres.Select(g => g.Genre).ToArray(),
            a.Email,
            a.Address.County,
            a.Address.Town,
            a.Latitude,
            a.Longitude));
}
