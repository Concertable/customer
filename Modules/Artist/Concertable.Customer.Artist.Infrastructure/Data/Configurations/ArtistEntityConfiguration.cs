using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Artist.Infrastructure.Data.Configurations;

internal class ArtistReadModelConfiguration : IEntityTypeConfiguration<ArtistReadModel>
{
    public void Configure(EntityTypeBuilder<ArtistReadModel> builder)
    {
        builder.ToTable("Artists", Schema.Name);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.HasMany(a => a.Genres)
            .WithOne(g => g.Artist)
            .HasForeignKey(g => g.ArtistId);
    }
}

internal class ArtistGenreReadModelConfiguration : IEntityTypeConfiguration<ArtistGenreReadModel>
{
    public void Configure(EntityTypeBuilder<ArtistGenreReadModel> builder)
    {
        builder.ToTable("ArtistGenres", Schema.Name);
        builder.HasKey(x => new { x.ArtistId, x.Genre });
    }
}
