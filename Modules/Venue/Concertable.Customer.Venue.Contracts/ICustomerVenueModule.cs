namespace Concertable.Customer.Venue.Contracts;

public interface ICustomerVenueModule
{
    Task<VenueSummary?> GetSummaryAsync(int venueId, CancellationToken ct = default);
}

public record VenueSummary(
    int Id,
    string Name,
    string County,
    string Town,
    double Latitude,
    double Longitude);
