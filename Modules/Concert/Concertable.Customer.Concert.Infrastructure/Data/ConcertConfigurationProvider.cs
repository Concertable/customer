using Concertable.Customer.Concert.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConcertReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new ConcertGenreReadModelConfiguration());
    }
}
