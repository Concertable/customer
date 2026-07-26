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
            .Subscribe<ConcertChangedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<ConcertPostedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<CustomerReviewSubmittedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<TicketPurchasedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<ArtistChangedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<VenueChangedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<ArtistRatingUpdatedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<VenueRatingUpdatedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<ConcertRatingUpdatedEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<CredentialRegisteredEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<PaymentSucceededEvent>(AppHostConstants.ServiceNames.Customer)
            .Subscribe<PaymentFailedEvent>(AppHostConstants.ServiceNames.Customer);
}
