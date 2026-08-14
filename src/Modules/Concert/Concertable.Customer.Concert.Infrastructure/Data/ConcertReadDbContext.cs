using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertReadDbContext(
    DbContextOptions<ConcertReadDbContext> options,
    ConcertConfigurationProvider provider)
    : ReadDbContext(options, provider, Schema.Name);
