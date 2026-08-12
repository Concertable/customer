using Concertable.Customer.Preference.Domain.Entities;
using Concertable.DataAccess;

namespace Concertable.Customer.Preference.Application.Interfaces;

internal interface IPreferenceRepository : IRepository<PreferenceEntity>
{
    Task<bool> InsertAsync(PreferenceEntity preference);
    Task<PreferenceEntity?> GetByUserIdAsync(Guid id);
    Task<IReadOnlyList<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<Genre> genres);
}
