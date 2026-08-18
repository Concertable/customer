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

Mirror of the B2B item in `api/Concertable.B2B/TECH_DEBT.md`. See [`plans/platform/SPLIT_TIME_E2E_STRATEGY.md`](../../plans/platform/SPLIT_TIME_E2E_STRATEGY.md).

---

## MED

### Web composes module infrastructure outside each API boundary

`Concertable.Customer.Web/Program.cs` directly registers the Concert, Ticket, Review, User,
Preference, Venue, and Artist infrastructure modules. It also calls separate API registration only
for User and Preference, while the other module controllers are discovered implicitly. The host must
therefore know which internal runtime registration belongs behind each HTTP module, and its project
directly references all seven `*.Infrastructure` projects.

**Resolves when:** each `AddXApi(IConfiguration)` extension composes its own `AddXModule` registration
and controller surface, `Concertable.Customer.Web` calls only those API extensions, and the Web project
removes every direct module-Infrastructure reference. Add an architecture guard that rejects direct
`Modules/*/*.Infrastructure` references from Web hosts so the boundary cannot regress.

---

### Preference module lacks `.Contracts` project

Concert and Ticket gained their `.Contracts` projects (`IConcertModule`, `ITicketModule`); Preference is the last module without one. No cross-module caller reaches into Preference today, so this is latent.

**Resolves when:** Preference gains a `Concertable.Customer.Preference.Contracts` csproj with `IPreferenceModule` + summary DTOs the moment another module needs it; internal types stay `internal`.

---

### Missing test projects for Artist, Venue, Preference

`Concertable.Customer.Artist`, `Concertable.Customer.Venue`, and `Concertable.Customer.Preference` have no Unit or Integration test projects.

**Resolves when:** Each gains at minimum an Integration tests project following the pattern in `Modules/Review/Tests/` or `Modules/Ticket/Tests/`.

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
