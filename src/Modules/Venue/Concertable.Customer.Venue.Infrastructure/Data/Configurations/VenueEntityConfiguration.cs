using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Venue.Infrastructure.Data.Configurations;

internal sealed class VenueEntityConfiguration : IEntityTypeConfiguration<VenueEntity>
{
    public void Configure(EntityTypeBuilder<VenueEntity> builder)
    {
        builder.ToTable(Schema.Tables.Venues, Schema.Name);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.OwnsAddress(v => v.Address);
    }
}
