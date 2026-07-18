using Concertable.Customer.Preference.Domain.Entities;
using Concertable.DataAccess;

namespace Concertable.Customer.Preference.Application.Interfaces;

internal interface IPreferenceRepository : IRepository<PreferenceEntity>
{
    Task<PreferenceEntity?> GetByUserIdAsync(Guid id);
    Task<IEnumerable<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<Genre> genres);
}
