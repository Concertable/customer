# Concertable.Customer.TestKit

Black-box HTTP helpers and wire models for tests that exercise the public Customer service. The package
contains no Customer runtime, persistence, repository, or Aspire implementation and does not provision
dependencies.

Construct `CustomerTestClient` with an `HttpClient` whose base address is the Customer service root. Consumers
can purchase a ticket through the public API and read the resulting upcoming-ticket projection.
