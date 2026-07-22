using Concertable.Customer.Concert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Concert.Infrastructure.Data.Configurations;

internal sealed class ConcertEntityConfiguration : IEntityTypeConfiguration<ConcertEntity>
{
    public void Configure(EntityTypeBuilder<ConcertEntity> builder)
    {
        builder.ToTable(Schema.Tables.Concerts, Schema.Name);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.ComplexProperty(c => c.Period, p =>
        {
            p.Property(x => x.Start).HasColumnName("Period_Start");
            p.Property(x => x.End).HasColumnName("Period_End");
        });

        builder.HasMany(c => c.Genres)
            .WithOne(g => g.Concert)
            .HasForeignKey(g => g.ConcertId);
    }
}

internal sealed class ConcertGenreEntityConfiguration : IEntityTypeConfiguration<ConcertGenreEntity>
{
    public void Configure(EntityTypeBuilder<ConcertGenreEntity> builder)
    {
        builder.ToTable(Schema.Tables.ConcertGenres, Schema.Name);
        builder.HasKey(x => new { x.ConcertId, x.Genre });
    }
}
