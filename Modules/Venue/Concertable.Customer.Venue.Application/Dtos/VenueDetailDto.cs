namespace Concertable.Customer.Venue.Application.Dtos;

public sealed record VenueDetailDto(
    int Id,
    string Name,
    string About,
    string BannerUrl,
    string Avatar,
    double Rating,
    string County,
    string Town,
    string Email,
    double Latitude,
    double Longitude);
