using Concertable.Auth.Contracts.Events;
using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Payment.Contracts.Events;

namespace Concertable.Customer.Hosting;

public static class CustomerTopology
{
    public static AsbTopology AddCustomerTopology(this AsbTopology topology) =>
        topology
            .Subscribe<ConcertChangedEvent>(CustomerConstants.ServiceName)
            .Subscribe<ConcertPostedEvent>(CustomerConstants.ServiceName)
            .Subscribe<CustomerReviewSubmittedEvent>(CustomerConstants.ServiceName)
            .Subscribe<TicketPurchasedEvent>(CustomerConstants.ServiceName)
            .Subscribe<ArtistChangedEvent>(CustomerConstants.ServiceName)
            .Subscribe<VenueChangedEvent>(CustomerConstants.ServiceName)
            .Subscribe<ArtistRatingUpdatedEvent>(CustomerConstants.ServiceName)
            .Subscribe<VenueRatingUpdatedEvent>(CustomerConstants.ServiceName)
            .Subscribe<ConcertRatingUpdatedEvent>(CustomerConstants.ServiceName)
            .Subscribe<CredentialRegisteredEvent>(CustomerConstants.ServiceName)
            .Subscribe<PaymentSucceededEvent>(CustomerConstants.ServiceName)
            .Subscribe<PaymentFailedEvent>(CustomerConstants.ServiceName)
            .Queue<SendTicketEmailCommand>(CustomerConstants.ServiceName);
}
