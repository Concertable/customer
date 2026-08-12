using Concertable.Customer.Artist.Infrastructure.Data;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal abstract class ReadRepository<TEntity>(ArtistReadDbContext context)
    : ReadRepository<TEntity, ArtistReadDbContext, int>(context)
    where TEntity : class, IIdEntity;
