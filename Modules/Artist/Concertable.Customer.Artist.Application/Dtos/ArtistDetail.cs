namespace Concertable.Customer.Artist.Application.Dtos;

public sealed record ArtistDetail(
    int Id,
    string Name,
    string About,
    string BannerUrl,
    string Avatar,
    double Rating,
    IReadOnlyCollection<Genre> Genres,
    string Email,
    string County,
    string Town,
    double Latitude,
    double Longitude);
