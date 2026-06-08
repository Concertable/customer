using Concertable.Customer.Artist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Artist.Infrastructure.Data.Configurations;

internal sealed class ArtistEntityConfiguration : IEntityTypeConfiguration<ArtistEntity>
{
    public void Configure(EntityTypeBuilder<ArtistEntity> builder)
    {
        builder.ToTable(Schema.Tables.Artists, Schema.Name);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.HasMany(a => a.Genres)
            .WithOne(g => g.Artist)
            .HasForeignKey(g => g.ArtistId);
    }
}

internal sealed class ArtistGenreEntityConfiguration : IEntityTypeConfiguration<ArtistGenreEntity>
{
    public void Configure(EntityTypeBuilder<ArtistGenreEntity> builder)
    {
        builder.ToTable(Schema.Tables.ArtistGenres, Schema.Name);
        builder.HasKey(x => new { x.ArtistId, x.Genre });
    }
}
