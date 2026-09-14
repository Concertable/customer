using Concertable.Customer.Preference.Application.DTOs;
using Concertable.Customer.Preference.Application.Errors;
using Concertable.Customer.Preference.Application.Requests;

namespace Concertable.Customer.Preference.Application.Interfaces;

internal interface IPreferenceService
{
    Task<Option<PreferenceDto>> GetByUserIdAsync(Guid userId);
    Task<Option<PreferenceDto>> GetByUserAsync();
    Task<IReadOnlyList<PreferenceDto>> GetAsync();
    Task<Result<PreferenceDto, CreatePreferenceError>> CreateAsync(PreferenceRequest request, Guid? userId = null);
    Task<Result<PreferenceDto, UpdatePreferenceError>> UpdateAsync(int id, PreferenceRequest request);
    Task<IReadOnlyList<Guid>> GetUserIdsByLocationAndGenresAsync(double latitude, double longitude, IEnumerable<Genre> genres);
}
