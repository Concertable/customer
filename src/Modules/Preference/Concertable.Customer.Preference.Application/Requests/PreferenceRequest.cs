
namespace Concertable.Customer.Preference.Application.Requests;

internal sealed record PreferenceRequest
{
    public int RadiusKm { get; init; }
    public IReadOnlyList<Genre> Genres { get; init; } = [];
}
