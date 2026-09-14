using Concertable.Contracts.Enums;
using Concertable.Customer.Preference.Domain.Entities;

namespace Concertable.Customer.Seed.Infrastructure.Factories;

public static class PreferenceFactory
{
    public static PreferenceEntity Create(Guid userId, double radiusKm, IEnumerable<Genre> genres) =>
        PreferenceEntity.Create(userId, radiusKm, genres);
}
