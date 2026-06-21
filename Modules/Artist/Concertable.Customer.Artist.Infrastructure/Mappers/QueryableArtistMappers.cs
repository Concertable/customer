using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Artist.Domain.Entities;

namespace Concertable.Customer.Artist.Infrastructure.Mappers;

internal static class QueryableArtistMappers
{
    public static IQueryable<ArtistSummary> ToSummary(this IQueryable<ArtistEntity> query) =>
        query.Select(a => new ArtistSummary(
            a.Id,
            a.Name,
            a.Avatar,
            a.AverageRating,
            a.County,
            a.Town,
            a.Genres.Select(g => g.Genre).ToArray()));

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
            a.County,
            a.Town,
            a.Latitude,
            a.Longitude));
}
