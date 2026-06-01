
namespace Concertable.Customer.Preference.Application.Requests;

internal sealed record CreatePreferenceRequest
{
    public int RadiusKm { get; set; }
    public IReadOnlyList<Genre> Genres { get; set; } = [];
}
