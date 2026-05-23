using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Application.Requests;

namespace Concertable.Customer.Preference.Application.Interfaces;

internal interface IPreferenceService
{
    Task<PreferenceDto?> GetByUserIdAsync(Guid userId);
    Task<PreferenceDto?> GetByUserAsync();
    Task<IEnumerable<PreferenceDto>> GetAsync();
    Task<PreferenceDto> CreateAsync(CreatePreferenceRequest request, Guid? userId = null);
    Task<PreferenceDto> UpdateAsync(PreferenceDto preferenceDto);
    Task<IReadOnlyCollection<Guid>> GetUserIdsByLocationAndGenresAsync(double latitude, double longitude, IEnumerable<Genre> genres);
}
