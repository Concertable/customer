namespace Concertable.Customer.Venue.Application.Dtos;

public record VenueDetailDto(
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
