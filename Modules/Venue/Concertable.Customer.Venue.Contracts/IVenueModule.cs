namespace Concertable.Customer.Venue.Contracts;

public interface IVenueModule
{
    Task<VenueSummary?> GetSummaryAsync(int venueId, CancellationToken ct = default);
}

public sealed record VenueSummary(
    int Id,
    string Name,
    string County,
    string Town,
    double Latitude,
    double Longitude);
