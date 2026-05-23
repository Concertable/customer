using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Concert.Infrastructure.Data.Configurations;

internal class ConcertEntityConfiguration : IEntityTypeConfiguration<ConcertEntity>
{
    public void Configure(EntityTypeBuilder<ConcertEntity> builder)
    {
        builder.ToTable("Concerts", Schema.Name);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.OwnsOne(c => c.Period, p =>
        {
            p.Property(x => x.Start).HasColumnName("Period_Start");
            p.Property(x => x.End).HasColumnName("Period_End");
        });

        builder.HasMany(c => c.Genres)
            .WithOne(g => g.Concert)
            .HasForeignKey(g => g.ConcertId);
    }
}

internal class ConcertGenreEntityConfiguration : IEntityTypeConfiguration<ConcertGenreEntity>
{
    public void Configure(EntityTypeBuilder<ConcertGenreEntity> builder)
    {
        builder.ToTable("ConcertGenres", Schema.Name);
        builder.HasKey(x => new { x.ConcertId, x.Genre });
    }
}
