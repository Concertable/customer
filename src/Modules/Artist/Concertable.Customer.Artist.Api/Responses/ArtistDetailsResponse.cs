using Concertable.Contracts;

namespace Concertable.Customer.Artist.Api.Responses;

public sealed record ArtistDetailsResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string About { get; init; }
    public required string BannerUrl { get; init; }
    public required string Avatar { get; init; }
    public double Rating { get; init; }
    public IReadOnlyCollection<Genre> Genres { get; init; } = [];
    public required string Email { get; init; }
    public required string County { get; init; }
    public required string Town { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
