using Concertable.Customer.Review.Domain.Entities;
using Concertable.Customer.Seed.Contracts;

namespace Concertable.Customer.Seed.Infrastructure.Factories;

public static class ReviewFactory
{
    public static ReviewEntity Create(ReviewSeedSpec spec) =>
        ReviewEntity
            .Create(spec.TicketId, spec.Stars, spec.Details, spec.Email, spec.ArtistId, spec.VenueId, spec.ConcertId)
            .Match(
                success: review => review,
                failure: _ => throw new InvalidOperationException("Seed review must be valid."));
}
