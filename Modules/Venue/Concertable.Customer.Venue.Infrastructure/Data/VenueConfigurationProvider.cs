using Concertable.Customer.Venue.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal sealed class VenueConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VenueReadModelConfiguration());
    }
}
