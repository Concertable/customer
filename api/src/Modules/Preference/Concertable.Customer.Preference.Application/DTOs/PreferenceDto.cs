
namespace Concertable.Customer.Preference.Application.DTOs;

internal sealed record PreferenceDto
{
    public int Id { get; init; }
    public Guid UserId { get; init; }
    public int RadiusKm { get; init; }
    public IReadOnlyList<Genre> Genres { get; init; } = [];
}
