using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Preference.Infrastructure.Data.Configurations;

internal sealed class PreferenceEntityConfiguration : IEntityTypeConfiguration<PreferenceEntity>
{
    public void Configure(EntityTypeBuilder<PreferenceEntity> builder)
    {
        builder.ToTable(Schema.Tables.Preferences, Schema.Name);
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
