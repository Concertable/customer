# Concertable.Customer — Technical Debt

When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## HIGH

### `TicketPurchasedEvent` not consumed by B2B/Search; `TicketRefundedEvent` not published

`TicketPurchasedEvent : IIntegrationEvent` now exists in `Concertable.Customer.Ticket.Contracts` — `TicketEntity.Purchase` raises `TicketPurchasedDomainEvent` (one per ticket), bridged to the bus via the outbox, registered as `Publishes<TicketPurchasedEvent>()` in `Program.cs`. Customer's own Concert module consumes it (`TicketPurchasedHandler` decrements `AvailableTickets`). Still missing from plan §6:

- B2B.Workers does not subscribe — no `ConcertSalesProjection` (sold-count / gross-revenue for dashboards + settlement math).
- Search.Workers does not subscribe — no "X tickets left" counts.
- `TicketRefundedEvent` does not exist (no refund flow yet).

**Resolves when:** B2B.Workers and Search.Workers subscribe and handle (+ their topology subscriptions on `event-ticketpurchasedevent`), and a refund flow publishes `TicketRefundedEvent`.

---

### E2E boots the whole real fleet from source references (won't survive the repo split)

`Concertable.Customer.E2ETests/AppFixture.cs` launches the Customer AppHost via
`DistributedApplicationTestingBuilder`, composing **real** Payment + Auth + Search through
`Projects.Concertable_*` *source* references. Fine in the monorepo, but it's full-fleet E2E run from
inside one service's repo — it conflates two test tiers and breaks at the repo split. E2E must never
stub Payment (stubbing defeats E2E); the fix is to split tiers by *where they run*:

**Resolves when:**
- **Per-repo (every PR):** Customer keeps only **integration** tests, with adapter services faked
  behind their contracts — Payment via `MockCustomerPaymentClient` against `Payment.Contracts` — plus
  **consumer-driven contract tests**. No Payment source/runtime needed.
- **Full-fleet system E2E (rare / pre-release, centralised):** stands up the real fleet from
  **published container images** (`AddProject<Projects.Concertable_Payment_Web>()` →
  `AddContainer("payment", "<registry>/payment:<version>")`), and moves out of Customer's repo into a
  system/deployment pipeline.

Mirror of the B2B item in `api/Concertable.B2B/TECH_DEBT.md`. See [`plans/SPLIT_TIME_E2E_STRATEGY.md`](../../plans/SPLIT_TIME_E2E_STRATEGY.md).

---

## MED

### Preference module lacks `.Contracts` project

Concert and Ticket gained their `.Contracts` projects (`IConcertModule`, `ITicketModule`); Preference is the last module without one. No cross-module caller reaches into Preference today, so this is latent.

**Resolves when:** Preference gains a `Concertable.Customer.Preference.Contracts` csproj with `IPreferenceModule` + summary DTOs the moment another module needs it; internal types stay `internal`.

---

### Missing test projects for Artist, Venue, Preference

`Concertable.Customer.Artist`, `Concertable.Customer.Venue`, and `Concertable.Customer.Preference` have no Unit or Integration test projects.

**Resolves when:** Each gains at minimum an Integration tests project following the pattern in `Modules/Review/Tests/` or `Modules/Ticket/Tests/`.

---

## LOW

### Customer has no DataAccess layer — design-time factory base parked in Seed.Infrastructure

B2B has `Concertable.B2B.DataAccess.Infrastructure` (referenced by every B2B module's Infrastructure
project); Customer has no equivalent — its module Infrastructure projects reference the shared
`Concertable.DataAccess.Infrastructure` package directly plus the in-closure
`Concertable.Customer.Seed.Infrastructure`. So the design-time `DesignTimeConfiguration` +
`CustomerDesignTimeDbContextFactory` base (single-sourcing the 7 Customer factories, and pulling a
`Microsoft.EntityFrameworkCore.SqlServer` ref into the seed project) landed in `Seed.Infrastructure` —
the only Customer-wide in-closure home available — which is a semantic mismatch (it's not seeding).

**Resolves when:** Customer gains a `Concertable.Customer.DataAccess.Infrastructure` (mirroring B2B) and
the design-time factory base + `DesignTimeConfiguration` move there. See `plans/CONFIG_AND_DEPLOYMENT.md`.

---

### Read repositories don't default to no-tracking

`ConcertReadRepository.GetDtoAsync` needed an ad-hoc `.AsNoTracking()` (EF throws when a projection carries a whole owned instance like `Period` on a tracking query), and the other read repos rely on projections happening to be untracked. Reads through a `ReadRepository<T>` should never track — the per-call opt-out is backwards.

**Resolves when:** the `ReadRepository<T>` base applies `AsNoTracking` to its query root so every derived read repo inherits it, and the ad-hoc call in `ConcertReadRepository` is removed. NOT context-wide `UseQueryTrackingBehavior(NoTracking)` — the projection handlers write through the same module contexts and need tracked queries.

---

### Ticket list reads load full entities (incl. `QrCode` blobs) instead of projecting

`TicketService.GetUserUpcomingAsync` / `GetUserHistoryAsync` materialise whole `TicketEntity` rows and map in memory (`tickets.ToDtos()`), hauling the `QrCode byte[]` blob for every ticket in a list view rather than a queryable projection. The empty-string masks that used to ride this path are gone: `UserEmail` was dropped from `TicketDto` (web reads it from nowhere; mobile `TicketDetailScreen` now reads the signed-in email from `useAuthStore`), the mapper no longer takes an email parameter, and `TicketPaymentProcessor` fail-closes on `meta["fromUserEmail"]`. What remains is pure efficiency — and it's blocked by an SPA coupling: both surfaces read `qrCode` straight off the list DTO (web `TicketCard` → `QrPopover`, mobile `<QRCode value={ticket.qrCode}>`), so `QrCode` can't simply be excluded from a projection.

**Resolves when:** the list reads become `IQueryable<TicketEntity>` projections that exclude `QrCode`, AND the SPA fetches the QR lazily per ticket (the read path already exists — `GetQrCodeByIdAsync` on the ticket repository) instead of reading it from the list DTO.
