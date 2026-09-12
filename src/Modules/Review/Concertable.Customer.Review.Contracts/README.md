# Concertable.Customer.Review.Contracts

The cross-service review contract surface published by the Concertable Customer service. It carries
`CustomerReviewSubmittedEvent`, the integration event B2B consumes — the one reverse data-flow from
Customer back to B2B.

Contracts only: no Customer runtime, persistence or handler implementation. Consumers subscribe to the
event through their own messaging transport.
