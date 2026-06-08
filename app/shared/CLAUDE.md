# @customer/shared — customer-only cross-platform core

## Consumed ONLY by web-customer and mobile-customer. Never by a manager/business app. Ever.

This is the customer-product sibling of `@concertable/shared` (which stays platform-agnostic AND
product-agnostic). Everything here makes Customer-service-authenticated calls or models
customer-only domain concepts:

- `lib/customerAxiosClient` — the Customer service axios instance + `configureCustomerApi`.
  Each customer app wraps it with its own token/401 interceptors (web: OIDC `userManager`;
  mobile: token storage). A 401 handler clearing the session is correct here — on a customer
  app, the only session it can clear is the customer's own stale one.
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
