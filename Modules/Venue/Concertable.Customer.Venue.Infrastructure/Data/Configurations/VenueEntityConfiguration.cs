using Concertable.Customer.Venue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Venue.Infrastructure.Data.Configurations;

internal class VenueEntityConfiguration : IEntityTypeConfiguration<VenueEntity>
{
    public void Configure(EntityTypeBuilder<VenueEntity> builder)
    {
        builder.ToTable("Venues", Schema.Name);
        builder.Property(v => v.Id).ValueGeneratedNever();
    }
}
