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
        topology.ForService(AppHostConstants.ServiceNames.Customer)
            .Subscribe<ConcertChangedEvent>()
            .Subscribe<ConcertPostedEvent>()
            .Subscribe<CustomerReviewSubmittedEvent>()
            .Subscribe<TicketPurchasedEvent>()
            .Subscribe<ArtistChangedEvent>()
            .Subscribe<VenueChangedEvent>()
            .Subscribe<ArtistRatingUpdatedEvent>()
            .Subscribe<VenueRatingUpdatedEvent>()
            .Subscribe<ConcertRatingUpdatedEvent>()
            .Subscribe<CredentialRegisteredEvent>()
            .Subscribe<PaymentSucceededEvent>()
            .Subscribe<PaymentFailedEvent>()
            .Topology;
}
