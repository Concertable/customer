using Concertable.Customer.Review.Contracts.Events;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Seed.Contracts;

public static class SeedSpecMappers
{
    extension(ReviewSeedSpec spec)
    {
        public CustomerReviewSubmittedEvent ToSubmittedEvent() => new(
            spec.TicketId,
            spec.ArtistId,
            spec.VenueId,
            spec.ConcertId,
            spec.Stars,
            spec.Email,
            spec.Details);

        public MessageEnvelope ToEnvelope() => new(
            spec.MessageId,
            MessageTypeAttribute.Resolve(typeof(CustomerReviewSubmittedEvent)),
            spec.OccurredAtUtc);
    }
}
