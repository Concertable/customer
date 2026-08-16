using Concertable.Customer.Artist.Domain.Entities;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal sealed class ArtistReadDbContext(
    DbContextOptions<ArtistReadDbContext> options,
    ArtistConfigurationProvider provider)
    : ReadDbContext(options, provider, Schema.Name), IArtistReadDbContext
{
    IQueryable<ArtistEntity> IArtistReadDbContext.Artists => Query<ArtistEntity>();
}
