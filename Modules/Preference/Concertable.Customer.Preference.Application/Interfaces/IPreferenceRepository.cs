using Concertable.Customer.Preference.Domain;
using Concertable.DataAccess;

namespace Concertable.Customer.Preference.Application.Interfaces;

internal interface IPreferenceRepository : IIdRepository<PreferenceEntity>
{
    new Task<IEnumerable<PreferenceEntity>> GetAllAsync();
    new Task<PreferenceEntity?> GetByIdAsync(int id);
    Task<PreferenceEntity?> GetByUserIdAsync(Guid id);
    Task<IEnumerable<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<Genre> genres);
}
