using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Preference.Infrastructure.Data.Configurations;

internal sealed class GenrePreferenceEntityConfiguration : IEntityTypeConfiguration<GenrePreferenceEntity>
{
    public void Configure(EntityTypeBuilder<GenrePreferenceEntity> builder)
    {
        builder.ToTable(Schema.Tables.GenrePreferences, Schema.Name);
        builder.HasOne(gp => gp.Preference)
            .WithMany(p => p.GenrePreferences)
            .HasForeignKey(gp => gp.PreferenceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(gp => new { gp.PreferenceId, gp.Genre }).IsUnique();
    }
}
