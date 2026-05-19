using Concertable.Customer.Profile.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Profile.Infrastructure.Data;

internal sealed class ProfileConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerProfileEntityConfiguration());
    }
}
