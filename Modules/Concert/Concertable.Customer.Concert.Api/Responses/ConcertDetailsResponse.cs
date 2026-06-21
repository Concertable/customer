using Concertable.Contracts;
using Concertable.Customer.Concert.Application.DTOs;

namespace Concertable.Customer.Concert.Api.Responses;

public sealed record ConcertDetailsResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string About { get; init; }
    public string? BannerUrl { get; init; }
    public string? Avatar { get; init; }
    public double Rating { get; init; }
    public decimal Price { get; init; }
    public int TotalTickets { get; init; }
    public int AvailableTickets { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime? DatePosted { get; init; }
    public required ConcertVenue Venue { get; init; }
    public required ConcertArtist Artist { get; init; }
    public IReadOnlyCollection<Genre> Genres { get; init; } = [];
}
