using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Domain.Entities;

namespace Concertable.Customer.Preference.Application.Mappers;

internal static class PreferenceMappers
{
    public static PreferenceDto ToDto(this PreferenceEntity preference) => new()
    {
        Id = preference.Id,
        RadiusKm = (int)preference.RadiusKm,
        UserId = preference.UserId,
        Genres = preference.GenrePreferences.Select(gp => gp.Genre).ToList()
    };

    public static IEnumerable<PreferenceDto> ToDtos(this IEnumerable<PreferenceEntity> preferences) =>
        preferences.Select(p => p.ToDto());
}
