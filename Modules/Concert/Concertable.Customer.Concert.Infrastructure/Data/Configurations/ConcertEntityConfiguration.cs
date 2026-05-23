using Concertable.Customer.Concert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Concert.Infrastructure.Data.Configurations;

internal class ConcertReadModelConfiguration : IEntityTypeConfiguration<ConcertReadModel>
{
    public void Configure(EntityTypeBuilder<ConcertReadModel> builder)
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

internal class ConcertGenreReadModelConfiguration : IEntityTypeConfiguration<ConcertGenreReadModel>
{
    public void Configure(EntityTypeBuilder<ConcertGenreReadModel> builder)
    {
        builder.ToTable("ConcertGenres", Schema.Name);
        builder.HasKey(x => new { x.ConcertId, x.Genre });
    }
}
