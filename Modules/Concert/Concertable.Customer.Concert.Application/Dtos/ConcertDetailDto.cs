namespace Concertable.Customer.Concert.Application.Dtos;

public record ConcertDetailDto(
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
    ConcertVenueDto Venue,
    ConcertArtistDto Artist,
    IReadOnlyCollection<Genre> Genres);

public record ConcertVenueDto(
    int Id,
    string Name,
    string County,
    string Town,
    double Latitude,
    double Longitude);

public record ConcertArtistDto(
    int Id,
    string Name,
    string? Avatar,
    double Rating,
    string County,
    string Town,
    IReadOnlyCollection<Genre> Genres);
