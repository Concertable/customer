using Microsoft.EntityFrameworkCore;
using CustomerReadDbContext = Concertable.Customer.DataAccess.Infrastructure.ReadDbContext;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertReadDbContext(
    DbContextOptions<ConcertReadDbContext> options,
    ConcertConfigurationProvider provider)
    : CustomerReadDbContext(options, provider, Schema.Name);
