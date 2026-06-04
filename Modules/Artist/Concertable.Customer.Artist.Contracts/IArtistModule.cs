using Concertable.Contracts;

namespace Concertable.Customer.Artist.Contracts;

public interface IArtistModule
{
    Task<ArtistSummary?> GetSummaryAsync(int artistId, CancellationToken ct = default);
}

public sealed record ArtistSummary(
    int Id,
    string Name,
    string? Avatar,
    double Rating,
    string County,
    string Town,
    IReadOnlyCollection<Genre> Genres);
