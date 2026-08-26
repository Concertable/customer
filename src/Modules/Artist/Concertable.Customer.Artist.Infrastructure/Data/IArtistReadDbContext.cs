using Concertable.Customer.Artist.Domain.Entities;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal interface IArtistReadDbContext
{
    IQueryable<ArtistEntity> Artists { get; }
}
