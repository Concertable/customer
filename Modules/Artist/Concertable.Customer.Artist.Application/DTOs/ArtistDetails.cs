using Concertable.Kernel;

namespace Concertable.Customer.Artist.Application.DTOs;

public sealed record ArtistDetails(
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
    double Longitude) : IAddress;
