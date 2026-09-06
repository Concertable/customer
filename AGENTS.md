# Concertable.Customer

Data service — the fan/buyer marketplace side. Inherits root [`AGENTS.md`](../../AGENTS.md); internal design → [`ARCHITECTURE.md`](./ARCHITECTURE.md) (read first, don't duplicate).

## Stay an agent/marketplace-facilitator, never the principal

Purchase/pricing/refund work must keep Concertable a facilitator: the venue is merchant-of-record, funds route to the venue's connected account, VAT applies only to our fee, prices are never set unilaterally. All-in pricing (fees in the headline price, no drip at checkout) and CRA/DMCCA refund rights are hard requirements. Detail → [`LEGAL_REQUIREMENTS.md`](./LEGAL_REQUIREMENTS.md).

## Artist/Venue data is projected twice

An `ArtistChangedEvent`/`VenueChangedEvent` (and the `*RatingUpdatedEvent` variants) feeds **both** the module `*Entity` handler and the Concert module's `*ReadModel` handler. Change a projection in both places or they drift.

## Tickets are webhook-minted, not returned by `/purchase`

`POST /purchase` and `POST /checkout` create an on-session Payment operation addressed by a whole `PaymentOperationReference`. A `TicketEntity` is created later by `TicketPaymentProcessor` on `PaymentSucceededEvent`, after the exact Customer operation-type guard succeeds. No ticket exists synchronously at purchase, and no provider identifier or Payment metadata crosses into Customer.
