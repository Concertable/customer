using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertReadDbContext(
    DbContextOptions<ConcertReadDbContext> options,
    ConcertConfigurationProvider provider)
    : ReadDbContext(options, provider, Schema.Name)
{
    public IQueryable<ConcertEntity> Concerts => Set<ConcertEntity>();
    public IQueryable<VenueReadModel> VenueReadModels => Set<VenueReadModel>();
    public IQueryable<ArtistReadModel> ArtistReadModels => Set<ArtistReadModel>();
}
