using System.ComponentModel;
using Concertable.Kernel;

namespace Concertable.Customer.Concert.Contracts;

public interface IConcertModule
{
    Task<ConcertDto?> GetByIdAsync(int concertId, CancellationToken ct = default);
}

[DisplayName(DisplayNames.Concert)]
public sealed record ConcertDto(
    int Id,
    string Name,
    decimal Price,
    DateRange Period,
    DateTime? DatePosted,
    int AvailableTickets,
    int ArtistId,
    string ArtistName,
    int VenueId,
    string VenueName,
    Guid PayeeUserId,
    Guid PayeeOwnerId);
