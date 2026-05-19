using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Concertable.Customer.Review.Infrastructure.Data.Configurations;

internal class ReviewEntityConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("Reviews", Schema.Name);
        builder.HasIndex(r => r.TicketId).IsUnique();
        builder.HasIndex(r => r.ConcertId);
        builder.HasIndex(r => r.ArtistId);
        builder.HasIndex(r => r.VenueId);
    }
}
