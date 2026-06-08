using Concertable.Contracts;

namespace Concertable.Customer.Concert.Application.Dtos;

public sealed record ConcertDetail(
    int Id,
    string Name,
    string About,
    string? BannerUrl,
    string? Avatar,
    double Rating,
    decimal Price,
    int TotalTickets,
    int AvailableTickets,
    DateTime StartDate,
    DateTime EndDate,
    DateTime? DatePosted,
    ConcertVenue Venue,
    ConcertArtist Artist,
    IReadOnlyCollection<Genre> Genres);

public sealed record ConcertVenue(
    int Id,
    string Name,
    string County,
    string Town,
    double Latitude,
    double Longitude);

public sealed record ConcertArtist(
    int Id,
    string Name,
    string? Avatar,
    double Rating,
    string County,
    string Town,
    IReadOnlyCollection<Genre> Genres);
