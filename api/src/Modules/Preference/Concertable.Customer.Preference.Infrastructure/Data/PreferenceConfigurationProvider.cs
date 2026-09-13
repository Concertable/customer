using Concertable.Customer.Preference.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Preference.Infrastructure.Data;

internal sealed class PreferenceConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PreferenceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GenrePreferenceEntityConfiguration());
    }
}
