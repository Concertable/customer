# Concertable.Customer — Architecture

> Cross-service plan and design rationale: [`api/docs/MICROSERVICES_ARCHITECTURE.md`](../docs/MICROSERVICES_ARCHITECTURE.md)
> Internal module rules: [`api/docs/MODULAR_MONOLITH_RULES.md`](../docs/MODULAR_MONOLITH_RULES.md)
> Outstanding gaps: [`TECH_DEBT.md`](./TECH_DEBT.md)

---

## Bounded context

Customer owns the fan/buyer side of Concertable: tickets sold, reviews of past concerts, customer preferences, and customer profiles. When a customer browses concerts, venue details, or artist profiles, those reads route through `Concertable.Search` — Customer.Api does not serve browse endpoints. Customer is **write-light**: most traffic is event consumption (projecting upstream B2B data) and ticket purchase / review submission.

---

## Host topology

| Project | Kind | Purpose |
|---|---|---|
| `Concertable.Customer.Web` | ASP.NET Core HTTP host | All controllers + ASB event consumers (in-process). Single deployable — no separate Workers host. |
| `Concertable.Customer.AppHost` | Aspire AppHost | Local-dev orchestrator only. |

**Database:** `CustomerDb` (SQL Server). Per-module DbContexts: `ArtistDbContext`, `ConcertDbContext`, `PreferenceDbContext`, `ReviewDbContext`, `TicketDbContext`, `UserDbContext`, `VenueDbContext` + `OutboxDbContext`, `InboxDbContext`. All auto-migrated on non-Production startup.

No `ReadDbContext` aggregate — Customer is small enough that cross-module reads use the existing per-module DbContexts directly.

---

## Modules

All modules live under `Modules/`. Each follows the `Concertable.Customer.<Module>.*` naming convention.

### Canonical modules (Customer owns the write path)

| Module | Canonical entities | Projects |
|---|---|---|
| **Ticket** | `TicketEntity` — Id (Guid), UserId, ConcertId, QrCode, PurchaseDate, plus purchase-time snapshot fields: ConcertName, Price, Period, ArtistId/Name, VenueId/Name, HasReview | Api, Application, Domain, Infrastructure, IntegrationTests, UnitTests — **no Contracts** (see TECH_DEBT) |
| **Review** | `ReviewEntity` — TicketId, ConcertId, ArtistId, VenueId, Stars (1–5), Details. Raises `ReviewCreatedDomainEvent` → published as `CustomerReviewSubmittedEvent`. | Api, Application, **Contracts**, Domain, Infrastructure, IntegrationTests, UnitTests |
| **Preference** | `PreferenceEntity` — UserId, RadiusKm, `HashSet<GenrePreferenceEntity>` | Api, Application, Domain, Infrastructure — **no Contracts** |
| **User** | `UserEntity` — Id (Auth `sub`), Email, optional `Point Location`, optional `Address`. This is the customer profile — there is no separate `CustomerProfileEntity`. Created from `CredentialRegisteredEvent`. | Api, Application, **Contracts**, Domain, Infrastructure, IntegrationTests, UnitTests |

### Read-model modules (populated from B2B events; no canonical writes)

| Module | Read model(s) | Event handlers |
|---|---|---|
| **Concert** | `ConcertReadModel`, `ConcertGenreReadModel` (file: `ConcertEntity.cs` — misleading name) | `ConcertProjectionHandler` ← `ConcertChangedEvent`; `ConcertRatingProjectionHandler` ← `ConcertRatingUpdatedEvent`; `ConcertPostedNotificationHandler` ← `ConcertPostedEvent` (Preference module) |
| **Artist** | `ArtistReadModel`, `ArtistGenreReadModel` | `ArtistProjectionHandler` ← `ArtistChangedEvent`; `ArtistRatingProjectionHandler` ← `ArtistRatingUpdatedEvent` |
| **Venue** | `VenueReadModel` | `VenueProjectionHandler` ← `VenueChangedEvent`; `VenueRatingProjectionHandler` ← `VenueRatingUpdatedEvent` |

> Note: read-model files under these modules are named `*Entity.cs` but contain classes named `*ReadModel`. Don't be misled — they have no canonical write path. See TECH_DEBT for the rename.

These tables are empty until upstream B2B events arrive. Under the umbrella `Concertable.AppHost`, real B2B publishes them. Under standalone `Concertable.Customer.AppHost` (dev), the `Concertable.B2B.Seed.Simulator` Worker is registered as an Aspire resource and stands in for B2B — publishing the same events from the canonical fixture, so projection state is identical either way. See [`../Concertable.B2B/Concertable.B2B.Seed.Simulator/CLAUDE.md`](../Concertable.B2B/Concertable.B2B.Seed.Simulator/CLAUDE.md) for the pattern.

---

## Integration events

Transport: Azure Service Bus (`concertable-customer` service name). Wired in `Concertable.Customer.Web/Program.cs`.

### Published

| Event | Defined in | When raised |
|---|---|---|
| `CustomerReviewSubmittedEvent` | `Concertable.Customer.Review.Contracts.Events` | `ReviewCreatedDomainEvent` fires on `ReviewEntity` creation; in-process bridge publishes to bus |

**Not yet published (TECH_DEBT):** `TicketPurchasedEvent`, `TicketRefundedEvent`. The inverse-direction flow (Customer → B2B/Search for sold-count) is not wired.

### Consumed

| Event | Source | Handler(s) |
|---|---|---|
| `ConcertChangedEvent` | B2B | `ConcertProjectionHandler` |
| `ConcertPostedEvent` | B2B | `ConcertPostedNotificationHandler` (Preference — notifies matching users) |
| `ConcertRatingUpdatedEvent` | B2B | `ConcertRatingProjectionHandler` |
| `VenueChangedEvent` | B2B | `VenueProjectionHandler` |
| `VenueRatingUpdatedEvent` | B2B | `VenueRatingProjectionHandler` |
| `ArtistChangedEvent` | B2B | `ArtistProjectionHandler` |
| `ArtistRatingUpdatedEvent` | B2B | `ArtistRatingProjectionHandler` |
| `CredentialRegisteredEvent` | Auth | `UserCreationHandler` — creates `UserEntity` |
| `PaymentSucceededEvent` | Payment | `TicketPaymentProcessor` |
| `PaymentFailedEvent` | Payment | `TicketPaymentFailedProcessor` |
| `CustomerReviewSubmittedEvent` | Self | Flips `TicketEntity.HasReview = true` |

All consumed events use `InboxMessageEntity` deduplication keyed by `(MessageId, ConsumerName)`.

---

## Sync calls out

| Target | Client | Usage |
|---|---|---|
| `Concertable.Payment` | `Concertable.Payment.Client` | Create payment intent on ticket purchase; refund ops |
| `Concertable.Auth` | JWT Bearer middleware | Token validation; `client_credentials` for service-to-service tokens |

No sync calls to B2B. Browse/detail reads go to `Concertable.Search` from the SPA directly — Customer.Api is not a proxy for Search.

---

## Authentication

- JWT Bearer, audience `concertable.customer.api`
- `client_credentials` client: `concertable-customer`, scope `payment:write`
- **No role claims in tokens.** Any token for audience `concertable.customer.api` with a `UserEntity` row for its `sub` is treated as a Customer.

---

## Internal architecture

Customer is a modular monolith inside the service. Rules in `api/docs/MODULAR_MONOLITH_RULES.md` apply:

- Cross-module calls: `IXModule` facade only (in `<Module>.Contracts`) — except Concert/Ticket/Preference which have no Contracts project yet (TECH_DEBT)
- Per-module DbContext, owns its own tables
- Intra-service events: in-process `IEventRaiser` — no bus involvement for intra-Customer flows
- Module-owned `IEntityTypeConfiguration<T>` in `<Module>.Infrastructure/Data/Configurations/`
- `InternalsVisibleTo` chains; module Application interfaces stay `internal`

---

## Tech stack

.NET 9 · EF Core + SQL Server · Azure Service Bus (emulator in dev) · `Concertable.Messaging` (Outbox/Inbox/Transport) · NetTopologySuite (geometry) · Aspire (`Concertable.ServiceDefaults`) · QRCoder + QuestPDF (`Ticket.Infrastructure` — ticket QR and PDF generation) · `Concertable.Shared.{Blob,Email,Geocoding,Imaging,Pdf}`

---

## What is NOT in this service

| Concern | Lives in |
|---|---|
| Concert workflow state machine, booking, settlement | `Concertable.B2B` |
| Venue/Artist/Concert canonical writes | `Concertable.B2B` |
| Browse / autocomplete / search endpoints | `Concertable.Search` |
| Stripe API keys, webhook receiver, payout ledger | `Concertable.Payment` |
| Identity authority (`sub`, password hash, email verification) | `Concertable.Auth` |
