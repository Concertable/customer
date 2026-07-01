using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Concert.Infrastructure.Data.Configurations;

internal sealed class VenueReadModelConfiguration : IEntityTypeConfiguration<VenueReadModel>
{
    public void Configure(EntityTypeBuilder<VenueReadModel> builder)
    {
        builder.ToTable(Schema.Tables.VenueReadModels, Schema.Name);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.OwnsAddress(v => v.Address);
    }
}
