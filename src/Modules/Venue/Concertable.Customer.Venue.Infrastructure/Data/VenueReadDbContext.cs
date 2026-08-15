using Microsoft.EntityFrameworkCore;
using CustomerReadDbContext = Concertable.Customer.DataAccess.Infrastructure.ReadDbContext;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueReadDbContext(
    DbContextOptions<VenueReadDbContext> options,
    VenueConfigurationProvider provider)
    : CustomerReadDbContext(options, provider, Schema.Name);
