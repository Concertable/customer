
namespace Concertable.Customer.Preference.Application.Requests;

internal record CreatePreferenceRequest
{
    public int RadiusKm { get; set; }
    public IReadOnlyList<Genre> Genres { get; set; } = [];
}
