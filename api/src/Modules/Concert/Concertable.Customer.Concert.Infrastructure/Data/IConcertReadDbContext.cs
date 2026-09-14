using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal interface IConcertReadDbContext
{
    IQueryable<ConcertEntity> Concerts { get; }
    IQueryable<ArtistReadModel> ArtistReadModels { get; }
    IQueryable<VenueReadModel> VenueReadModels { get; }
}
