namespace Concertable.Customer.Venue.Application.DTOs;

public sealed record VenueDetails(
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
