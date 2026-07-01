using Concertable.Customer.Artist.Infrastructure.Data;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal abstract class ReadRepository<TEntity>(ArtistDbContext context)
    : ReadRepository<TEntity, ArtistDbContext, int>(context)
    where TEntity : class, IIdEntity;
