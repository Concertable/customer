using Concertable.Customer.Venue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Venue.Infrastructure.Data.Configurations;

internal class VenueReadModelConfiguration : IEntityTypeConfiguration<VenueReadModel>
{
    public void Configure(EntityTypeBuilder<VenueReadModel> builder)
    {
        builder.ToTable("Venues", Schema.Name);
        builder.Property(v => v.Id).ValueGeneratedNever();
    }
}
