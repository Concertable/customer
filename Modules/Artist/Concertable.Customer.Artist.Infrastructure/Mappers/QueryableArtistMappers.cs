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
}
