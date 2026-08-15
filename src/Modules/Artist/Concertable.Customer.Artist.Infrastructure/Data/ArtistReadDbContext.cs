using Microsoft.EntityFrameworkCore;
using CustomerReadDbContext = Concertable.Customer.DataAccess.Infrastructure.ReadDbContext;

namespace Concertable.Customer.Artist.Infrastructure.Data;

internal sealed class ArtistReadDbContext(
    DbContextOptions<ArtistReadDbContext> options,
    ArtistConfigurationProvider provider)
    : CustomerReadDbContext(options, provider, Schema.Name);
