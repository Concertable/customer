# @concertable/customer — customer-only cross-platform core

Inherits [`../../AGENTS.md`](../../AGENTS.md) (frontend conventions + patterns).

Published as package `@concertable/customer`; every public entry point is beneath its `./shared` export.

## Consumed ONLY by web-customer and mobile-customer. Never by a manager/business app. Ever.

This is the customer-product sibling of `@concertable/shared` (which stays platform-agnostic AND
product-agnostic). Everything here makes Customer-service-authenticated calls or models
customer-only domain concepts:

- `lib/customerClient` — the bare Customer-service axios instance. Each customer app enhances it
  with `configureClient(...).withAuth(...)` — web via OIDC `userManager`, mobile via token storage.
  A 401 handler clearing the session is correct here — on a customer app, the only session it can
  clear is the customer's own stale one.
- `features/tickets` — purchase/checkout/upcoming/history + `Ticket`/`TicketCheckout` types.
- `features/preferences` — preference CRUD (talks to the own-app `api`, which for customer
  apps IS the Customer service).
- `features/reviews` — the eligibility + create api (reads live in the apps' own backends and
  stay in `@concertable/shared`-typed web/shared code). The hooks live in web/customer — one
  consumer — and own their auth gate there.
- `features/notifications` — `TicketPurchasedPayload` + its SignalR handler hook.

The test for new code: *"is this only meaningful when the caller is a customer?"* If a manager
app could legitimately use it, it belongs in `@concertable/shared`. If it's web-only or
mobile-only, it belongs in that app (or `web/shared` when all four web sites can run it).

Adding this package as a dependency to a business/manager app is the bug to never introduce —
that's how manager tokens ended up on Customer-service calls (routine 401s, band-aided
interceptors) before the boundary split.
