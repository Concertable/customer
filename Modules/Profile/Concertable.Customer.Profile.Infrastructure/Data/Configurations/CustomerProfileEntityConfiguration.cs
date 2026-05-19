using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Profile.Infrastructure.Data.Configurations;

internal sealed class CustomerProfileEntityConfiguration : IEntityTypeConfiguration<CustomerProfileEntity>
{
    public void Configure(EntityTypeBuilder<CustomerProfileEntity> builder)
    {
        builder.ToTable("CustomerProfiles", Schema.Name);
        builder.HasKey(x => x.Sub);
        builder.Property(x => x.Sub).ValueGeneratedNever();
    }
}
