using Concertable.Customer.Review.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Review.Infrastructure.Data;

internal sealed class ReviewConfigurationProvider : IEntityTypeConfigurationProvider
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ReviewEntityConfiguration());
    }
}
