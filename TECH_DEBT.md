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

Mirror of the B2B item in `api/Concertable.B2B/TECH_DEBT.md`. Tracked by [`plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](../../plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md), stages 3 and 4.

---

## MED

### Preference module lacks `.Contracts` project

Concert and Ticket gained their `.Contracts` projects (`IConcertModule`, `ITicketModule`); Preference is the last module without one. No cross-module caller reaches into Preference today, so this is latent.

**Resolves when:** Preference gains a `Concertable.Customer.Preference.Contracts` csproj with `IPreferenceModule` + summary DTOs the moment another module needs it; internal types stay `internal`.

---

## LOW

### `DateRange` mapped as `ComplexProperty` on Ticket but `OwnsOne` elsewhere

`DateRange` is a value object (no identity), so it belongs as a `ComplexProperty` — as the repo already
maps its other value objects (`ESignature`, `InvoiceAmounts`, `InvoiceParty`). `TicketEntity.Period`
was moved to `ComplexProperty` to fix a real bug: `OwnsOne` models it as an owned *entity*, and
`TicketService.CompleteAsync` hands the same `concert.Period` instance to every ticket in a
multi-ticket purchase — EF forbids one owned instance having N owners, so the 2nd+ ticket saved with
NULL `Period_Start` and the purchase 500'd. The other four `DateRange` mappings (B2B
Concert/Contract/Opportunity, Customer Concert) stay `OwnsOne`: they never share an instance so they
don't hit the bug, and converting them breaks their projection-handler unit tests, which use the EF
**InMemory** provider — it can't materialize a complex type (`KeyNotFoundException` on
`Period#DateRange.Start` in its query shaper).

**Resolves when:** the InMemory-based projection-handler unit tests (Customer Concert, B2B Concert)
move to a provider that supports complex types (SQLite in-memory); then all `DateRange` mappings become
`ComplexProperty` and no value object is mapped as an owned entity. Same root cause as the `AsNoTracking`
item below.

---

### Ticket list reads load full entities (incl. `QrCode` blobs) instead of projecting

`TicketService.GetUserUpcomingAsync` / `GetUserHistoryAsync` materialise whole `TicketEntity` rows and map in memory (`tickets.ToDtos()`), hauling the `QrCode byte[]` blob for every ticket in a list view rather than a queryable projection. The empty-string masks that used to ride this path are gone: `UserEmail` was dropped from `TicketDto` (web reads it from nowhere; mobile `TicketDetailScreen` now reads the signed-in email from `useAuthStore`), the mapper no longer takes an email parameter, and `TicketPaymentProcessor` fail-closes on `meta["fromUserEmail"]`. What remains is pure efficiency — and it's blocked by an SPA coupling: both surfaces read `qrCode` straight off the list DTO (web `TicketCard` → `QrPopover`, mobile `<QRCode value={ticket.qrCode}>`), so `QrCode` can't simply be excluded from a projection.

**Resolves when:** the list reads become `IQueryable<TicketEntity>` projections that exclude `QrCode`, AND the SPA fetches the QR lazily per ticket (the read path already exists — `GetQrCodeByIdAsync` on the ticket repository) instead of reading it from the list DTO.

### Rate-limit policy names are raw literals in Customer controllers (no service-wide constants home)

The `[EnableRateLimiting(...)]` attributes on Customer controllers carry raw strings
(`"public-read"`, `"purchase"`, `"review"`) rather than the `RateLimitPolicies` constants, because those
constants live in `Concertable.Customer.Web` (the host — the only place that can register the policies) and
the module `*.Api` projects cannot reference the host. Unlike B2B, whose policy constants sit in the
universally-referenced `Concertable.B2B.Tenant.Contracts`, Customer has no low-level project that every
module `*.Api` shares, so there is nowhere reachable to put a shared constant. A typo in a literal fails
fast at endpoint execution (no matching policy) and the `public-read` + `review` names are exercised by the
rate-limit integration trip tests; only `purchase` is unpinned by a test.

**Resolves when:** a low-level Customer project the module `*.Api` projects (and the host) all reference —
mirroring B2B's `Tenant.Contracts` — holds `RateLimitPolicies`, and the Customer controller attributes
reference the constants instead of literals. Needs a project-topology decision (whether Customer should
gain such a shared assembly, and its name/placement).
