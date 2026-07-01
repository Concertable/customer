public static class CustomerTopology
{
    public static AsbTopology AddCustomerTopology(this AsbTopology topology) =>
        topology
            .Subscribe("event-concertchangedevent",          "customer-concert-changed",       "concertable-customer")
            .Subscribe("event-concertpostedevent",           "customer-concert-posted",         "concertable-customer")
            .Subscribe("event-customerreviewsubmittedevent", "customer-review-submitted",       "concertable-customer")
            .Subscribe("event-ticketpurchasedevent",         "customer-ticket-purchased",       "concertable-customer")
            .Subscribe("event-artistchangedevent",           "customer-artist-changed",         "concertable-customer")
            .Subscribe("event-venuechangedevent",            "customer-venue-changed",          "concertable-customer")
            .Subscribe("event-artistratingupdatedevent",     "customer-artist-rating-updated",  "concertable-customer")
            .Subscribe("event-venueratingupdatedevent",      "customer-venue-rating-updated",   "concertable-customer")
            .Subscribe("event-concertratingupdatedevent",    "customer-concert-rating-updated", "concertable-customer")
            .Subscribe("event-credentialregisteredevent",    "customer-credential-registered",  "concertable-customer")
            .Subscribe("event-paymentsucceededevent",        "customer-payment-succeeded",      "concertable-customer")
            .Subscribe("event-paymentfailedevent",           "customer-payment-failed",         "concertable-customer");
}
