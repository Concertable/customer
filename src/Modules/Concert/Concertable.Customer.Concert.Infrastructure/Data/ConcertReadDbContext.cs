using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertReadDbContext(
    DbContextOptions<ConcertReadDbContext> options,
    ConcertConfigurationProvider provider)
    : ReadDbContext(options, provider, Schema.Name), IConcertReadDbContext
{
    IQueryable<ConcertEntity> IConcertReadDbContext.Concerts => Query<ConcertEntity>();
    IQueryable<ArtistReadModel> IConcertReadDbContext.ArtistReadModels => Query<ArtistReadModel>();
    IQueryable<VenueReadModel> IConcertReadDbContext.VenueReadModels => Query<VenueReadModel>();
}
