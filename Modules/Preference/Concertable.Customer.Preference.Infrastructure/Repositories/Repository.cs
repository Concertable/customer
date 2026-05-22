using Concertable.Customer.Preference.Infrastructure.Data;

namespace Concertable.Customer.Preference.Infrastructure.Repositories;

internal abstract class Repository<TEntity>(PreferenceDbContext context)
    : Repository<TEntity, PreferenceDbContext>(context)
    where TEntity : class, IIdEntity;
