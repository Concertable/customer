using Concertable.Auth.Contracts.Events;
using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Payment.Contracts.Events;

public static class CustomerTopology
{
    public static AsbTopology AddCustomerTopology(this AsbTopology topology) =>
        topology
            .Subscribe<ConcertChangedEvent>("customer-concert-changed",       "concertable-customer")
            .Subscribe<ConcertPostedEvent>("customer-concert-posted",         "concertable-customer")
            .Subscribe<CustomerReviewSubmittedEvent>("customer-review-submitted",       "concertable-customer")
            .Subscribe<TicketPurchasedEvent>("customer-ticket-purchased",       "concertable-customer")
            .Subscribe<ArtistChangedEvent>("customer-artist-changed",         "concertable-customer")
            .Subscribe<VenueChangedEvent>("customer-venue-changed",          "concertable-customer")
            .Subscribe<ArtistRatingUpdatedEvent>("customer-artist-rating-updated",  "concertable-customer")
            .Subscribe<VenueRatingUpdatedEvent>("customer-venue-rating-updated",   "concertable-customer")
            .Subscribe<ConcertRatingUpdatedEvent>("customer-concert-rating-updated", "concertable-customer")
            .Subscribe<CredentialRegisteredEvent>("customer-credential-registered",  "concertable-customer")
            .Subscribe<PaymentSucceededEvent>("customer-payment-succeeded",      "concertable-customer")
            .Subscribe<PaymentFailedEvent>("customer-payment-failed",         "concertable-customer");
}
